using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;

namespace NovaStay.Infrastructure.Payments;

public sealed class InvoiceSettlementService(HostContext db)
{
    /// <summary>Invoice, provider attempt, receipts and notification are saved atomically.</summary>
    public async Task SettleAsync(Invoice invoice, string source, Guid? confirmedBy, string reference, string? note, CancellationToken ct)
    {
        if (invoice.Status == "Paid") return;
        if (invoice.Status is not ("Unpaid" or "AwaitingConfirmation")) throw new InvalidOperationException("Trạng thái hóa đơn không hợp lệ.");
        if (note?.Length > 500 || reference.Length > 100) throw new ArgumentException("Thông tin xác nhận quá dài.");
        var now = DateTime.UtcNow;
        var owner = await db.Organizations.Where(x => x.Id == invoice.OrganizationId).Select(x => x.OwnerAccountId).SingleAsync(ct);
        var resident = await db.Residents.SingleOrDefaultAsync(x => x.Id == invoice.ResidentId, ct);
        invoice.Status = "Paid"; invoice.PaidAt = now; invoice.UpdatedAt = now;
        invoice.ConfirmedByAccountId = confirmedBy; invoice.ConfirmationNote = note?.Trim();
        invoice.PaymentSource = source; invoice.PaymentReference = reference;
        var category = await db.IncomeCategories.FirstOrDefaultAsync(x => x.OrganizationId == invoice.OrganizationId && x.CategoryCode == "MONTHLY_BILL", ct);
        if (category == null)
        {
            category = new IncomeCategory { Id = Guid.NewGuid(), OrganizationId = invoice.OrganizationId, CategoryCode = "MONTHLY_BILL", CategoryName = "Thu hóa đơn phòng", IsActive = true, CreatedAt = now };
            db.IncomeCategories.Add(category);
        }
        db.PaymentReceipts.Add(new PaymentReceipt
        {
            Id = Guid.NewGuid(), InvoiceId = invoice.Id, OrganizationId = invoice.OrganizationId, PropertyId = invoice.PropertyId,
            ResidentId = invoice.ResidentId, ReceiptNumber = "PT" + invoice.InvoiceNumber![2..], ReceiptType = "InvoicePayment",
            PaymentMethod = "BankTransfer", Amount = invoice.TotalAmount, PaidAt = now, ReferenceCode = reference,
            Status = "Confirmed", PayerName = resident?.FullName, Note = note?.Trim(), CreatedAt = now
        });
        db.IncomeReceipts.Add(new IncomeReceipt
        {
            Id = Guid.NewGuid(), OrganizationId = invoice.OrganizationId, PropertyId = invoice.PropertyId!.Value, RoomId = invoice.RoomId,
            ResidentId = invoice.ResidentId, IncomeCategoryId = category.Id, ReceiptNumber = "PT" + invoice.InvoiceNumber![2..],
            IncomeType = "Other", PayerName = resident?.FullName ?? "Người đại diện", Amount = invoice.TotalAmount,
            CollectedAt = now, PaymentMethod = "BankTransfer", Status = "Approved", ReferenceCode = invoice.InvoiceNumber,
            Description = $"Thu hóa đơn {invoice.InvoiceNumber}, kỳ {invoice.InvoicePeriod} ({source})", CreatedAt = now
        });
        db.PaymentNotificationOutbox.Add(new PaymentNotificationOutbox
        {
            Id = Guid.NewGuid(), InvoiceId = invoice.Id, OwnerAccountId = owner, ResidentAccountId = resident?.AccountId,
            PaymentSource = source, PaidAt = now, CreatedAt = now
        });
        await db.SaveChangesAsync(ct);
    }
}
