using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.Payments;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class BillingService(HostContext db, PaymentAccess? paymentAccess = null, InvoiceSettlementService? settlement = null)
{
    public const string BillType = "ManualTransfer";

    public async Task<Guid> OwnerOrganization(Guid accountId, CancellationToken ct = default)
    {
        var id = await db.Organizations.Where(x => x.OwnerAccountId == accountId).Select(x => x.Id).FirstOrDefaultAsync(ct);
        return id != Guid.Empty ? id : throw new UnauthorizedAccessException("Không tìm thấy tổ chức của chủ trọ.");
    }

    public async Task<BankAccountDto?> GetBank(Guid ownerId, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        return Bank(await LatestBank(org, ct));
    }

    private Task<BillingBankAccount?> LatestBank(Guid org, CancellationToken ct) => db.BillingBankAccounts
        .Where(x => x.OrganizationId == org).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(ct);

    public async Task<BankAccountDto> SaveBank(Guid ownerId, string bank, string number, string name,
        byte[]? image, string? contentType, bool removeQr, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        bank = (bank ?? "").Trim();
        number = string.Concat((number ?? "").Where(c => !char.IsWhiteSpace(c)));
        name = (name ?? "").Trim();
        if (bank.Length is < 1 or > 100)
            throw new ArgumentException("Tên ngân hàng phải có từ 1 đến 100 ký tự.");
        if (number.Length is < 1 or > 50 || number.Any(c => !char.IsAsciiLetterOrDigit(c)))
            throw new ArgumentException("Số tài khoản phải có từ 1 đến 50 chữ cái hoặc chữ số, không chứa dấu câu hay ký tự đặc biệt.");
        if (name.Length is < 1 or > 150)
            throw new ArgumentException("Tên chủ tài khoản phải có từ 1 đến 150 ký tự.");
        if (image != null) ValidateQr(image, contentType);
        var previous = await LatestBank(org, ct);
        var sameDestination = previous?.BankName == bank && previous.AccountNumber == number && previous.AccountName == name;
        var account = new BillingBankAccount
        {
            Id = Guid.NewGuid(), OrganizationId = org, BankName = bank, AccountNumber = number, AccountName = name,
            QrImage = removeQr ? null : image ?? (sameDestination ? previous?.QrImage : null),
            QrContentType = removeQr ? null : image != null ? contentType : sameDestination ? previous?.QrContentType : null,
            CreatedAt = DateTime.UtcNow
        };
        db.BillingBankAccounts.Add(account);
        await db.SaveChangesAsync(ct);
        return Bank(account)!;
    }

    public static void ValidateQr(byte[] data, string? type)
    {
        var png = data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10});
        var jpg = data.Length >= 3 && data[0] == 255 && data[1] == 216 && data[2] == 255;
        if (data.Length > 2 * 1024 * 1024 || !(type == "image/png" && png || type == "image/jpeg" && jpg))
            throw new ArgumentException("Ảnh QR phải là PNG hoặc JPG, tối đa 2 MB.");
    }

    public async Task<IReadOnlyList<object>> Properties(Guid ownerId, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        return await db.Properties.Where(x => x.OrganizationId == org).OrderBy(x => x.PropertyName)
            .Select(x => (object)new { x.Id, Name = x.PropertyName }).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BillingRoomDto>> Preview(Guid ownerId, Guid propertyId, string period, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        var start = ParsePeriod(period);
        var end = start.AddMonths(1).AddDays(-1);
        if (!await db.Properties.AnyAsync(x => x.Id == propertyId && x.OrganizationId == org, ct))
            throw new KeyNotFoundException("Không tìm thấy nhà trọ.");
        var rooms = await db.Rooms.AsNoTracking().Where(x => x.PropertyId == propertyId && !x.IsDeleted)
            .Include(x => x.Contracts).ThenInclude(x => x.Resident)
            .Include(x => x.RoomServices).ThenInclude(x => x.PropertyService)
            .Include(x => x.UtilityMeters).ThenInclude(x => x.PropertyService)
            .Include(x => x.UtilityMeters).ThenInclude(x => x.UtilityReadings)
            .AsSplitQuery().OrderBy(x => x.RoomNumber).ToListAsync(ct);
        var billed = await db.InvoiceLines.AsNoTracking().Where(x => x.BillingOrganizationId == org && x.BillingPeriod == period)
            .Select(x => new { x.BillingRoomId, x.SourceType, x.SourceId }).ToListAsync(ct);
        var result = new List<BillingRoomDto>();
        foreach (var room in rooms)
        {
            var contracts = room.Contracts.Where(x => x.OrganizationId == org && x.Status == "Active" && x.StartDate <= end && x.EndDate >= start).ToList();
            var errors = new List<string>();
            if (contracts.Count != 1) errors.Add("Phòng cần đúng một hợp đồng có hiệu lực trong kỳ để xác định người đại diện.");
            var contract = contracts.Count == 1 ? contracts[0] : null;
            var lines = new List<BillingLineDto>();
            bool Already(string type, Guid id) => billed.Any(x => x.BillingRoomId == room.Id && x.SourceType == type && x.SourceId == id);
            if (!Already("Rent", room.Id) && room.BasePrice > 0)
                lines.Add(new("rent", "Tiền phòng", 1, room.BasePrice, Money(room.BasePrice)));
            var services = room.RoomServices.Where(x => x.IsActive && x.PropertyService.IsActive).Select(x => x.PropertyService)
                .Concat(room.UtilityMeters.Where(x => x.IsActive && x.PropertyService.IsActive).Select(x => x.PropertyService)).DistinctBy(x => x.Id);
            foreach (var service in services)
            {
                if (Already("Service", service.Id)) continue;
                var key = $"service:{service.Id}";
                var meters = room.UtilityMeters.Where(x => x.PropertyServiceId == service.Id && x.IsActive).ToList();
                if (meters.Count > 0)
                {
                    var readings = meters.Select(x => x.UtilityReadings.SingleOrDefault(r => r.BillingPeriod == period)).ToList();
                    if (readings.Any(x => x == null)) { errors.Add($"{key}|{service.ServiceName}: chưa ghi đủ chỉ số kỳ {period}."); continue; }
                    var quantity = readings.Sum(x => x!.Consumption);
                    // Readings supply consumption; lock the configured price only when issuing the invoice.
                    // This also corrects unbilled readings captured before the owner configured a price.
                    var unitPrice = room.RoomServices.FirstOrDefault(x => x.PropertyServiceId == service.Id && x.IsActive)?.PriceOverride
                        ?? service.DefaultPrice;
                    if (unitPrice < 0) { errors.Add($"{key}|{service.ServiceName}: đơn giá không hợp lệ."); continue; }
                    var amount = Money(quantity * unitPrice);
                    lines.Add(new(key, service.ServiceName + " (theo chỉ số đã ghi)", quantity,
                        unitPrice, amount));
                }
                else
                {
                    // Meter-based and per-use charges require actual usage; never guess a flat charge.
                    var unit = (service.Unit ?? "").ToLowerInvariant();
                    if (service.BillingCycle == "PerUse" || unit.Contains("kwh") || unit.Contains("m3") || unit.Contains("m³") || unit.Contains("khối") || unit.Contains("người"))
                    { errors.Add($"{key}|{service.ServiceName}: cần cấu hình đồng hồ hoặc định lượng trước khi thu."); continue; }
                    var price = room.RoomServices.First(x => x.PropertyServiceId == service.Id && x.IsActive).PriceOverride ?? service.DefaultPrice;
                    if (price < 0) { errors.Add($"{key}|{service.ServiceName}: đơn giá không hợp lệ."); continue; }
                    lines.Add(new(key, service.ServiceName, 1, price, Money(price)));
                }
            }
            result.Add(new(room.Id, room.RoomNumber, contract?.ResidentId, contract?.Resident.FullName, lines, errors));
        }
        return result;
    }

    public async Task<IReadOnlyList<BillDto>> Issue(Guid ownerId, IssueBillsRequest request, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        var start = ParsePeriod(request.Period);
        var mode = request.PaymentMode?.Trim();
        if (mode is not ("Manual" or "PayOS")) throw new ArgumentException("PaymentMode must be Manual or PayOS.");
        if (mode == "PayOS")
        {
            if (paymentAccess == null) throw new PaymentRuleException("PAYOS_CONFIGURATION_REQUIRED", "PayOS chưa được cấu hình.");
            paymentAccess.EnsureReady(ownerId);
        }
        if (request.DueDate < start || request.DueDate > start.AddYears(1)) throw new ArgumentException("Hạn thanh toán không hợp lệ.");
        if (request.RoomIds.Count is < 1 or > 500 || request.ChargeKeys.Count == 0) throw new ArgumentException("Chọn phòng và ít nhất một khoản thu.");
        var bank = mode == "Manual" ? await LatestBank(org, ct) ?? throw new InvalidOperationException("Cần cấu hình tài khoản nhận tiền trước.") : null;
        var preview = await Preview(ownerId, request.PropertyId, request.Period, ct);
        var invoices = new List<Invoice>();
        foreach (var roomId in request.RoomIds.Distinct())
        {
            var room = preview.SingleOrDefault(x => x.RoomId == roomId) ?? throw new ArgumentException("Phòng không thuộc nhà trọ đã chọn.");
            if (room.ResidentId == null) throw new InvalidOperationException($"Phòng {room.RoomNumber}: chưa xác định được người đại diện.");
            // A selected charge can be absent because it was already billed, is unavailable, or belongs to another room.
            var errors = room.Errors.Where(e => e.Contains('|') && request.ChargeKeys.Contains(e.Split('|')[0])).ToList();
            if (errors.Count > 0) throw new InvalidOperationException($"Phòng {room.RoomNumber}: {string.Join(" ", errors.Select(e => e.Split('|')[1]))}");
            var selected = room.Lines.Where(x => request.ChargeKeys.Contains(x.Key)).ToList();
            if (selected.Count == 0) throw new InvalidOperationException($"Phòng {room.RoomNumber}: không có khoản thu mới; có thể đã lập trong kỳ này.");
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(), OrganizationId = org, PropertyId = request.PropertyId, RoomId = roomId,
                ResidentId = room.ResidentId, InvoicePeriod = request.Period, InvoiceType = BillType,
                InvoiceNumber = "HD" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),
                DueDate = request.DueDate, Status = "Unpaid", IssuedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
                BillingBankAccountId = bank?.Id, PaymentMode = mode,
                RoomPrice = selected.Where(x => x.Key == "rent").Sum(x => x.Amount),
                ServicesPrice = selected.Where(x => x.Key != "rent").Sum(x => x.Amount),
                Subtotal = selected.Sum(x => x.Amount), TotalAmount = selected.Sum(x => x.Amount)
            };
            if (invoice.TotalAmount <= 0) throw new InvalidOperationException($"Phòng {room.RoomNumber}: tổng tiền phải lớn hơn 0.");
            var contracts = await db.Contracts.Where(x => x.RoomId == roomId && x.ResidentId == room.ResidentId && x.Status == "Active" && x.StartDate <= start.AddMonths(1).AddDays(-1) && x.EndDate >= start).ToListAsync(ct);
            invoice.ContractId = contracts.Single().Id;
            foreach (var line in selected)
                invoice.InvoiceLines.Add(new InvoiceLine
                {
                    Id = Guid.NewGuid(), BillingOrganizationId = org, BillingRoomId = roomId, BillingPeriod = request.Period,
                    SourceType = line.Key == "rent" ? "Rent" : "Service", SourceId = line.Key == "rent" ? roomId : Guid.Parse(line.Key[8..]),
                    LineType = line.Key == "rent" ? "Rent" : "Service", Description = line.Description,
                    Quantity = line.Quantity, UnitPrice = line.UnitPrice, LineAmount = line.Amount,
                    BillingStartDate = start, BillingEndDate = start.AddMonths(1).AddDays(-1), CreatedAt = DateTime.UtcNow,
                    IsDebit = true, DisplayOrder = invoice.InvoiceLines.Count
                });
            invoices.Add(invoice);
        }
        db.Invoices.AddRange(invoices);
        // One atomic save; unique charge index also protects concurrent requests.
        await db.SaveChangesAsync(ct);
        return await OwnerBills(ownerId, request.PropertyId, ct);
    }

    private IQueryable<Invoice> Bills() => db.Invoices.Where(x => x.InvoiceType == BillType)
        .Include(x => x.InvoiceLines).Include(x => x.BillingBankAccount).Include(x => x.Property).Include(x => x.Room).Include(x => x.Resident);

    public async Task<IReadOnlyList<BillDto>> OwnerBills(Guid ownerId, Guid? propertyId = null, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        var query = Bills().AsNoTracking().Where(x => x.OrganizationId == org);
        if (propertyId.HasValue) query = query.Where(x => x.PropertyId == propertyId);
        return await Project(query.OrderByDescending(x => x.CreatedAt)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BillDto>> ResidentBills(Guid accountId, CancellationToken ct = default) =>
        await Project(Bills().AsNoTracking().Where(x => x.Resident != null && x.Resident.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAt)).ToListAsync(ct);

    public async Task<PagedResult<BillDto>> OwnerBillsPaged(Guid ownerId, Guid? propertyId, int pageIndex, int pageSize,
        string? status, string? period, string? search, CancellationToken ct = default)
    {
        var offset = Pagination.Offset(pageIndex, pageSize);
        var org = await OwnerOrganization(ownerId, ct);
        var query = Bills().AsNoTracking().Where(x => x.OrganizationId == org);
        if (propertyId.HasValue)
        {
            if (!await db.Properties.AnyAsync(x => x.Id == propertyId && x.OrganizationId == org, ct))
                throw new KeyNotFoundException("Không tìm thấy nhà trọ.");
            query = query.Where(x => x.PropertyId == propertyId);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = new[] { "Unpaid", "AwaitingConfirmation", "Paid" }.FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException("status must be Unpaid, AwaitingConfirmation or Paid.");
            query = query.Where(x => x.Status == normalized);
        }
        if (!string.IsNullOrWhiteSpace(period)) { ParsePeriod(period); query = query.Where(x => x.InvoicePeriod == period); }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => (x.InvoiceNumber != null && x.InvoiceNumber.Contains(term)) ||
                (x.Resident != null && x.Resident.FullName.Contains(term)) || (x.Room != null && x.Room.RoomNumber.Contains(term)));
        }
        var count = await query.CountAsync(ct);
        var items = await Project(query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Skip(offset).Take(pageSize)).ToListAsync(ct);
        return new() { Items = items, TotalCount = count, PageIndex = pageIndex, PageSize = pageSize };
    }

    public async Task<BillDto> Report(Guid accountId, Guid id, CancellationToken ct = default)
    {
        var invoice = await Bills().SingleOrDefaultAsync(x => x.Id == id && x.Resident != null && x.Resident.AccountId == accountId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");
        if (invoice.PaymentMode == "PayOS") throw new PaymentRuleException("PAYOS_AUTOMATIC_CONFIRMATION", "Hóa đơn PayOS được xác nhận tự động từ giao dịch ngân hàng.");
        if (invoice.Status == "Unpaid")
        {
            invoice.Status = "AwaitingConfirmation"; invoice.TransferReportedAt = DateTime.UtcNow; invoice.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Map(invoice);
    }

    public async Task<BillDto> Confirm(Guid ownerId, Guid id, string? note, CancellationToken ct = default)
    {
        var org = await OwnerOrganization(ownerId, ct);
        var invoice = await Bills().SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == org, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");
        if (invoice.Status == "Paid") return Map(invoice);
        if (invoice.PaymentMode == "PayOS") throw new PaymentRuleException("PAYOS_AUTOMATIC_CONFIRMATION", "Hãy đối soát PayOS; không xác nhận thủ công hóa đơn đang dùng PayOS.");
        await (settlement ?? new InvoiceSettlementService(db)).SettleAsync(invoice, "Manual", ownerId, invoice.InvoiceNumber!, note, ct);
        return Map(invoice);
    }

    public async Task<(byte[] Image, string Type)> Qr(Guid accountId, Guid bankId, CancellationToken ct = default)
    {
        var bank = await db.BillingBankAccounts.SingleOrDefaultAsync(x => x.Id == bankId, ct);
        if (bank == null || bank.QrImage == null) throw new KeyNotFoundException("Chưa có ảnh QR.");
        var owner = await db.Organizations.AnyAsync(x => x.Id == bank.OrganizationId && x.OwnerAccountId == accountId, ct);
        var resident = await db.Invoices.AnyAsync(x => x.BillingBankAccountId == bankId && x.Resident != null && x.Resident.AccountId == accountId, ct);
        if (!owner && !resident) throw new KeyNotFoundException("Không tìm thấy ảnh QR.");
        return (bank.QrImage, bank.QrContentType!);
    }

    public static DateOnly ParsePeriod(string period) => DateOnly.TryParseExact(period + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date : throw new ArgumentException("Kỳ thu phải có dạng yyyy-MM.");
    private static decimal Money(decimal value) => decimal.Round(value, 0, MidpointRounding.AwayFromZero);
    private static BankAccountDto? Bank(BillingBankAccount? x) => x == null ? null : new(x.Id, x.BankName, x.AccountNumber, x.AccountName, x.QrImage != null);
    // Project metadata only; QR bytes are fetched through the authorized image endpoint on demand.
    private static IQueryable<BillDto> Project(IQueryable<Invoice> query) => query.Select(x => new BillDto(
        x.Id, x.PropertyId, x.Property != null ? x.Property.PropertyName : "", x.Room != null ? x.Room.RoomNumber : "",
        x.Resident != null ? x.Resident.FullName : "", x.InvoiceNumber ?? "", x.InvoicePeriod, x.DueDate,
        x.TotalAmount, x.Status ?? "Unpaid", x.TransferReportedAt, x.PaidAt, x.ConfirmationNote,
        x.BillingBankAccount == null ? null : new BankAccountDto(x.BillingBankAccount.Id, x.BillingBankAccount.BankName,
            x.BillingBankAccount.AccountNumber, x.BillingBankAccount.AccountName, x.BillingBankAccount.QrImage != null),
        x.InvoiceLines.OrderBy(l => l.DisplayOrder).Select(l => new BillingLineDto(l.SourceType == "Rent" ? "rent" : "service:" + (l.SourceId.HasValue ? l.SourceId.Value.ToString() : ""),
            l.Description, l.Quantity, l.UnitPrice, l.LineAmount)).ToList(), x.PaymentMode, x.PaymentSource));
    private static BillDto Map(Invoice x) => new(x.Id, x.PropertyId, x.Property?.PropertyName ?? "", x.Room?.RoomNumber ?? "",
        x.Resident?.FullName ?? "", x.InvoiceNumber ?? "", x.InvoicePeriod, x.DueDate, x.TotalAmount, x.Status ?? "Unpaid",
        x.TransferReportedAt, x.PaidAt, x.ConfirmationNote, Bank(x.BillingBankAccount), x.InvoiceLines.OrderBy(l => l.DisplayOrder)
            .Select(l => new BillingLineDto(l.SourceType == "Rent" ? "rent" : $"service:{l.SourceId}", l.Description, l.Quantity, l.UnitPrice, l.LineAmount)).ToList(), x.PaymentMode, x.PaymentSource);
}
