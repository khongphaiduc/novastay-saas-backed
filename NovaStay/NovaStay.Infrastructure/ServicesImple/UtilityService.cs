using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

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
        var meter = await _context.UtilityMeters
            .Include(m => m.PropertyService)
            .Include(m => m.UtilityReadings)
            .FirstOrDefaultAsync(m => m.Id == meterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy đồng hồ Id={meterId}.");

        // Tính chỉ số cũ: reading gần nhất hoặc InitialReading nếu chưa ghi lần nào
        var previousReading = meter.UtilityReadings
            .OrderByDescending(r => r.ReadingDate)
            .Select(r => (decimal?)r.CurrentReading)
            .FirstOrDefault() ?? meter.InitialReading;

        if (request.CurrentReading < previousReading)
            throw new InvalidOperationException(
                $"Chỉ số mới ({request.CurrentReading}) không được nhỏ hơn chỉ số cũ ({previousReading}).");

        // Kiểm tra kỳ ghi nhận đã tồn tại chưa
        var periodExists = meter.UtilityReadings
            .Any(r => r.BillingPeriod == request.BillingPeriod);
        if (periodExists)
            throw new InvalidOperationException(
                $"Kỳ '{request.BillingPeriod}' đã được ghi nhận cho đồng hồ này. Vui lòng kiểm tra lại.");

        var consumption = request.CurrentReading - previousReading;

        // Tìm đơn giá hiện tại từ UtilityTariff (nếu có), fallback về DefaultPrice của service
        var today = DateTime.UtcNow;
        var tariff = await _context.UtilityTariffs
            .Where(t => t.PropertyServiceId == meter.PropertyServiceId
                        && t.EffectiveFrom <= today
                        && (t.EffectiveTo == null || t.EffectiveTo >= today))
            .OrderByDescending(t => t.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        var unitPrice = tariff?.UnitPrice ?? meter.PropertyService.DefaultPrice;
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
        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
        if (!roomExists)
            throw new KeyNotFoundException($"Không tìm thấy phòng Id={request.RoomId}.");

        // Kiểm tra service tồn tại
        var service = await _context.PropertyServices
            .FirstOrDefaultAsync(s => s.Id == request.PropertyServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ Id={request.PropertyServiceId}.");

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
    private static UtilityMeterDto MapMeter(UtilityMeter m)
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
        };
    }

    private static UtilityReadingDto MapReading(UtilityReading r) => new()
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
