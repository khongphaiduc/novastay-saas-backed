using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class UtilityService : IUtilityService
{
    private readonly HostContext _context;

    public UtilityService(HostContext context)
    {
        _context = context;
    }

    // ─── GetMetersByRoom ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<UtilityMeterDto>> GetMetersByRoomAsync(
        Guid roomId, CancellationToken cancellationToken = default)
    {
        var meters = await _context.UtilityMeters
            .Include(m => m.PropertyService)
            .Include(m => m.UtilityReadings)
            .Where(m => m.RoomId == roomId && m.IsActive)
            .ToListAsync(cancellationToken);

        return meters.Select(MapMeter).ToList().AsReadOnly();
    }

    // ─── GetMetersByProperty ──────────────────────────────────────────────────
    public async Task<IReadOnlyList<UtilityMeterDto>> GetMetersByPropertyAsync(
        Guid propertyId, CancellationToken cancellationToken = default)
    {
        var meters = await _context.UtilityMeters
            .Include(m => m.PropertyService)
            .Include(m => m.Room)
            .Include(m => m.UtilityReadings)
            .Where(m => m.PropertyService.PropertyId == propertyId && m.IsActive)
            .ToListAsync(cancellationToken);

        return meters.Select(MapMeter).ToList().AsReadOnly();
    }

    // ─── RecordReading ────────────────────────────────────────────────────────
    public async Task<UtilityReadingDto> RecordReadingAsync(
        Guid meterId,
        RecordUtilityReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!DateTime.TryParseExact(request.BillingPeriod, "yyyy-MM", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            throw new InvalidOperationException("Kỳ ghi nhận phải có định dạng yyyy-MM.");
        if (request.CurrentReading < 0)
            throw new InvalidOperationException("Chỉ số mới không được âm.");

        var meter = await _context.UtilityMeters
            .Include(m => m.PropertyService)
            .Include(m => m.UtilityReadings)
            .FirstOrDefaultAsync(m => m.Id == meterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy đồng hồ Id={meterId}.");

        if (!meter.IsActive)
            throw new InvalidOperationException("Đồng hồ đã ngừng hoạt động.");
        // Kỳ đã có số: đây là thao tác "Sửa số đã chốt" -> cập nhật số thay vì tạo bản ghi mới.
        var existing = meter.UtilityReadings.FirstOrDefault(r => r.BillingPeriod == request.BillingPeriod);
        if (existing is not null)
            return await CorrectReadingAsync(meter, existing, request, cancellationToken);

        if (meter.UtilityReadings.Any(r => string.Compare(r.BillingPeriod, request.BillingPeriod, StringComparison.Ordinal) > 0))
            throw new InvalidOperationException("Không thể ghi chỉ số cho kỳ trước khi đồng hồ đã có số liệu ở kỳ sau.");

        // Tính chỉ số cũ: reading gần nhất hoặc InitialReading nếu chưa ghi lần nào
        var previousReading = meter.UtilityReadings
            .OrderByDescending(r => r.ReadingDate)
            .Select(r => (decimal?)r.CurrentReading)
            .FirstOrDefault() ?? meter.InitialReading;

        if (request.CurrentReading < previousReading)
            throw new InvalidOperationException(
                $"Chỉ số mới ({request.CurrentReading}) không được nhỏ hơn chỉ số cũ ({previousReading}).");

        var consumption = request.CurrentReading - previousReading;

        // Match the price owners actually configure in the service catalog, including a room override.
        var roomService = await _context.RoomServices.AsNoTracking().SingleOrDefaultAsync(
            x => x.RoomId == meter.RoomId && x.PropertyServiceId == meter.PropertyServiceId && x.IsActive,
            cancellationToken);
        var unitPrice = roomService?.PriceOverride ?? meter.PropertyService.DefaultPrice;
        if (unitPrice < 0) throw new InvalidOperationException("Đơn giá dịch vụ không được âm.");
        var amount = consumption * unitPrice;

        var reading = new UtilityReading
        {
            Id = Guid.NewGuid(),
            UtilityMeterId = meterId,
            ReadingDate = DateTime.UtcNow,
            BillingPeriod = request.BillingPeriod,
            PreviousReading = previousReading,
            CurrentReading = request.CurrentReading,
            Consumption = consumption,
            UnitPrice = unitPrice,
            Amount = amount,
            Status = "Recorded",
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
        };

        _context.UtilityReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);

        return MapReading(reading);
    }

    // Sửa số của kỳ đã chốt. Chỉ cho phép với kỳ gần nhất, vì sửa kỳ cũ sẽ làm sai
    // chỉ số cũ và tiêu thụ của kỳ sau. Hóa đơn được tính từ chỉ số tại thời điểm xuất.
    private async Task<UtilityReadingDto> CorrectReadingAsync(
        UtilityMeter meter,
        UtilityReading reading,
        RecordUtilityReadingRequest request,
        CancellationToken cancellationToken)
    {
        if (meter.UtilityReadings.Any(r => string.Compare(r.BillingPeriod, reading.BillingPeriod, StringComparison.Ordinal) > 0))
            throw new InvalidOperationException("Chỉ có thể sửa số của kỳ gần nhất. Kỳ sau đã có số liệu.");

        var previousReading = meter.UtilityReadings
            .Where(r => r.Id != reading.Id)
            .OrderByDescending(r => r.BillingPeriod)
            .Select(r => (decimal?)r.CurrentReading)
            .FirstOrDefault() ?? meter.InitialReading;

        if (request.CurrentReading < previousReading)
            throw new InvalidOperationException(
                $"Chỉ số mới ({request.CurrentReading}) không được nhỏ hơn chỉ số cũ ({previousReading}).");

        var roomService = await _context.RoomServices.AsNoTracking().SingleOrDefaultAsync(
            x => x.RoomId == meter.RoomId && x.PropertyServiceId == meter.PropertyServiceId && x.IsActive,
            cancellationToken);
        var unitPrice = roomService?.PriceOverride ?? meter.PropertyService.DefaultPrice;
        if (unitPrice < 0) throw new InvalidOperationException("Đơn giá dịch vụ không được âm.");

        var consumption = request.CurrentReading - previousReading;
        reading.PreviousReading = previousReading;
        reading.CurrentReading = request.CurrentReading;
        reading.Consumption = consumption;
        reading.UnitPrice = unitPrice;
        reading.Amount = consumption * unitPrice;
        reading.Note = request.Note;

        await _context.SaveChangesAsync(cancellationToken);
        return MapReading(reading);
    }

    // ─── GetReadingsByMeter ───────────────────────────────────────────────────
    public async Task<IReadOnlyList<UtilityReadingDto>> GetReadingsByMeterAsync(
        Guid meterId, CancellationToken cancellationToken = default)
    {
        var readings = await _context.UtilityReadings
            .Where(r => r.UtilityMeterId == meterId)
            .OrderByDescending(r => r.ReadingDate)
            .ToListAsync(cancellationToken);

        return readings.Select(MapReading).ToList().AsReadOnly();
    }

    // ─── CreateMeter ──────────────────────────────────────────────────────────
    public async Task<UtilityMeterDto> CreateMeterAsync(
        CreateUtilityMeterRequest request,
        CancellationToken cancellationToken = default)
    {
        // Kiểm tra phòng tồn tại
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng Id={request.RoomId}.");

        // Kiểm tra service tồn tại
        var service = await _context.PropertyServices
            .FirstOrDefaultAsync(s => s.Id == request.PropertyServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ Id={request.PropertyServiceId}.");

        if (service.PropertyId != room.PropertyId || !service.IsActive)
            throw new InvalidOperationException("Dịch vụ phải đang hoạt động và thuộc cùng cơ sở với phòng.");
        if (request.InitialReading < 0)
            throw new InvalidOperationException("Chỉ số ban đầu không được âm.");
        if (string.IsNullOrWhiteSpace(request.MeterType) || string.IsNullOrWhiteSpace(request.Unit))
            throw new InvalidOperationException("Vui lòng nhập loại đồng hồ và đơn vị tính.");

        // Không được có 2 đồng hồ cùng loại cho 1 phòng
        var duplicate = await _context.UtilityMeters.AnyAsync(
            m => m.RoomId == request.RoomId && m.PropertyServiceId == request.PropertyServiceId && m.IsActive,
            cancellationToken);
        if (duplicate)
            throw new InvalidOperationException("Phòng này đã có đồng hồ cho dịch vụ này rồi.");

        var meter = new UtilityMeter
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            PropertyServiceId = request.PropertyServiceId,
            MeterCode = request.MeterCode,
            MeterType = request.MeterType,
            Unit = request.Unit,
            InitialReading = request.InitialReading,
            IsActive = true,
            InstalledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        _context.UtilityMeters.Add(meter);
        await _context.SaveChangesAsync(cancellationToken);

        meter.PropertyService = service;
        return MapMeter(meter);
    }

    // ─── Mappers ──────────────────────────────────────────────────────────────
    internal static UtilityMeterDto MapMeter(UtilityMeter m)
    {
        var latest = m.UtilityReadings?
            .OrderByDescending(r => r.ReadingDate)
            .FirstOrDefault();

        return new UtilityMeterDto
        {
            Id = m.Id,
            RoomId = m.RoomId,
            PropertyServiceId = m.PropertyServiceId,
            MeterCode = m.MeterCode,
            MeterType = m.MeterType,
            Unit = m.Unit,
            InitialReading = m.InitialReading,
            IsActive = m.IsActive,
            ServiceName = m.PropertyService?.ServiceName ?? "",
            LatestReading = latest != null ? MapReading(latest) : null,
            Readings = m.UtilityReadings?.OrderByDescending(r => r.BillingPeriod).Select(MapReading).ToList()
                ?? new List<UtilityReadingDto>(),
        };
    }

    internal static UtilityReadingDto MapReading(UtilityReading r) => new()
    {
        Id = r.Id,
        UtilityMeterId = r.UtilityMeterId,
        ReadingDate = r.ReadingDate,
        BillingPeriod = r.BillingPeriod,
        PreviousReading = r.PreviousReading,
        CurrentReading = r.CurrentReading,
        Consumption = r.Consumption,
        UnitPrice = r.UnitPrice,
        Amount = r.Amount,
        Status = r.Status,
        Note = r.Note,
        CreatedAt = r.CreatedAt,
    };
}
