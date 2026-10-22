using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.ServicesImple;
using Microsoft.Extensions.Logging;

namespace NovaStay.Infrastructure.Payments;

public sealed class PaymentProcessingService(HostContext db, PayOsOptions options, PaymentAccess access,
    IPayOsGateway gateway, InvoiceSettlementService settlement, ILogger<PaymentProcessingService>? logger = null)
{
    public async Task<PaymentCapabilitiesDto> Capabilities(Guid account, CancellationToken ct)
    {
        if (!await db.Organizations.AnyAsync(x => x.OwnerAccountId == account, ct)) throw new UnauthorizedAccessException();
        return access.Capabilities(account);
    }

    private async Task<Invoice> Invoice(Guid account, Guid id, bool ownerOnly, CancellationToken ct) =>
        await db.Invoices.Include(x => x.Organization).Include(x => x.Resident)
            .SingleOrDefaultAsync(x => x.Id == id && x.InvoiceType == BillingService.BillType &&
                (x.Organization.OwnerAccountId == account || !ownerOnly && x.Resident != null && x.Resident.AccountId == account), ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");

    public async Task<InvoicePaymentStatusDto> Status(Guid account, Guid id, CancellationToken ct) =>
        await State(await Invoice(account, id, false, ct), ct);

    public async Task<InvoicePaymentStatusDto> CreateLink(Guid account, Guid id, CancellationToken ct)
    {
        var invoice = await Invoice(account, id, false, ct);
        if (invoice.PaymentMode != "PayOS") throw new PaymentRuleException("MANUAL_INVOICE", "Hóa đơn này dùng chuyển khoản thủ công.");
        access.EnsureReady(invoice.Organization.OwnerAccountId);
        if (invoice.Status == "Paid") throw new PaymentRuleException("INVOICE_ALREADY_PAID", "Hóa đơn đã thanh toán.");
        if (invoice.Status is not ("Unpaid" or "AwaitingConfirmation"))
            throw new PaymentRuleException("INVOICE_NOT_PAYABLE", "Hóa đơn không còn ở trạng thái được thanh toán.");
        if (invoice.TotalAmount <= 0 || invoice.TotalAmount != decimal.Truncate(invoice.TotalAmount) || invoice.TotalAmount > int.MaxValue)
            throw new PaymentRuleException("INVALID_PAYMENT_AMOUNT", "Số tiền hóa đơn không phù hợp với PayOS.");
        if (await db.PayOsPaymentAttempts.AnyAsync(x => x.InvoiceId == id && x.Status == "NeedsReview", ct))
            throw new PaymentRuleException("PAYMENT_NEEDS_REVIEW", "Hóa đơn có giao dịch cần đối soát. Không chuyển tiền thêm trước khi chủ trọ kiểm tra.");
        var attempt = await db.PayOsPaymentAttempts.SingleOrDefaultAsync(x => x.InvoiceId == id && x.IsActive, ct);
        if (attempt?.Status == "NeedsReview") throw new PaymentRuleException("PAYMENT_NEEDS_REVIEW", "Giao dịch cần chủ trọ đối soát trước khi thanh toán tiếp.");
        if (attempt?.Status == "Pending")
        {
            if (attempt.ExpiresAt > DateTime.UtcNow) return await State(invoice, ct);
            await Refresh(invoice, attempt, ct);
            if (invoice.Status == "Paid") return await State(invoice, ct);
            if (attempt.IsActive) throw new PaymentRuleException("PAYMENT_NOT_TERMINAL", "Đang chờ PayOS xác nhận hết hạn hoặc đối soát. Vui lòng thử lại sau.");
            attempt = null;
        }
        if (attempt == null)
        {
            attempt = new PayOsPaymentAttempt
            {
                Id = Guid.NewGuid(), InvoiceId = id, OrderCode = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8)) & ((1L << 52) - 1),
                ChannelOwnerAccountId = invoice.Organization.OwnerAccountId, Amount = invoice.TotalAmount,
                AccountNumber = options.ExpectedAccountNumber.Trim(),
                IsActive = true, Status = "Creating",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(options.LinkLifetimeMinutes)
            };
            if (attempt.OrderCode == 0) attempt.OrderCode = 1;
            db.PayOsPaymentAttempts.Add(attempt);
            await db.SaveChangesAsync(ct); // Persist identity before any provider call; retries keep this orderCode.
        }
        PayOsLink link;
        try { link = await gateway.CreateAsync(attempt.OrderCode, (int)attempt.Amount, id, attempt.ExpiresAt, ct); }
        catch (PaymentProviderException)
        {
            link = await gateway.GetAsync(attempt.OrderCode, ct) ?? throw new PaymentProviderException("PAYOS_CREATE_UNCERTAIN");
        }
        await db.Entry(attempt).ReloadAsync(ct);
        await db.Entry(invoice).ReloadAsync(ct); // A webhook can arrive while create is in flight.
        if (invoice.Status == "Paid") return await State(invoice, ct);
        if (link.OrderCode != attempt.OrderCode || link.Amount != attempt.Amount || string.IsNullOrWhiteSpace(link.Id) ||
            link.AccountNumber != null && link.AccountNumber != attempt.AccountNumber &&
                !CanBindVirtualAccount(attempt, link) ||
            attempt.PaymentLinkId != null && attempt.PaymentLinkId != link.Id)
        {
            await Review(attempt, "ProviderLinkMismatch", ct);
            throw new PaymentRuleException("PAYOS_CHANNEL_MISMATCH", "Kênh nhận tiền PayOS không khớp cấu hình. Cần chủ trọ kiểm tra.");
        }
        attempt.PaymentLinkId = link.Id;
        if (!string.IsNullOrWhiteSpace(link.AccountNumber)) attempt.AccountNumber = link.AccountNumber;
        attempt.AccountName = link.AccountName ?? attempt.AccountName; attempt.BankBin = link.BankBin ?? attempt.BankBin;
        attempt.CheckoutUrl = SafeCheckout(link.CheckoutUrl, link.Id);
        attempt.QrCode = link.QrCode ?? attempt.QrCode;
        attempt.Status = "Pending"; attempt.UpdatedAt = DateTime.UtcNow;
        await ApplyProvider(invoice, attempt, link, ct);
        return await State(invoice, ct);
    }

    public async Task<InvoicePaymentStatusDto> Reconcile(Guid owner, Guid id, CancellationToken ct)
    {
        var invoice = await Invoice(owner, id, true, ct);
        if (invoice.PaymentMode != "PayOS") throw new PaymentRuleException("MANUAL_INVOICE", "Hóa đơn này dùng chuyển khoản thủ công.");
        if (invoice.Status == "Paid") { await CancelOutstanding(invoice, ct); return await State(invoice, ct); }
        var attempt = await db.PayOsPaymentAttempts.Where(x => x.InvoiceId == id).OrderByDescending(x => x.Status == "NeedsReview")
            .ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        if (attempt != null) await Refresh(invoice, attempt, ct);
        return await State(invoice, ct);
    }

    private async Task Refresh(Invoice invoice, PayOsPaymentAttempt attempt, CancellationToken ct)
    {
        if (attempt.ReviewReason == "ProviderLinkMismatch")
            throw new PaymentRuleException("PAYMENT_NEEDS_REVIEW", "Cần kiểm tra danh tính kênh nhận tiền trước khi đối soát.");
        if (attempt.ChannelOwnerAccountId != options.DemoOwnerAccountId ||
            invoice.Organization.OwnerAccountId != attempt.ChannelOwnerAccountId || string.IsNullOrWhiteSpace(options.ApiKey))
            throw new PaymentRuleException("PAYOS_CONFIGURATION_REQUIRED", "Không có cấu hình kênh gốc để đối soát giao dịch.");
        var link = await gateway.GetAsync(attempt.OrderCode, ct);
        if (link == null) throw new PaymentProviderException("PAYOS_ORDER_NOT_FOUND");
        if (attempt.ReviewReason == "WebhookIdentityMismatch")
        {
            if (!HasVerifiedBankTransactions(invoice, attempt, link))
                throw new PaymentRuleException("PAYMENT_NEEDS_REVIEW", "PayOS chưa cung cấp đủ dữ liệu giao dịch khớp để xác nhận thanh toán.");
            await Paid(invoice, attempt, link.Transactions[0].Reference!, ct);
            return;
        }
        await ApplyProvider(invoice, attempt, link, ct);
    }

    private async Task ApplyProvider(Invoice invoice, PayOsPaymentAttempt attempt, PayOsLink link, CancellationToken ct)
    {
        if (link.OrderCode != attempt.OrderCode || link.Amount != attempt.Amount ||
            attempt.PaymentLinkId != null && attempt.PaymentLinkId != link.Id ||
            link.AccountNumber != null && link.AccountNumber != attempt.AccountNumber)
        { await Review(attempt, "ProviderLinkMismatch", ct); return; }
        attempt.PaymentLinkId ??= link.Id;
        attempt.CheckoutUrl ??= SafeCheckout(link.CheckoutUrl, link.Id);
        attempt.QrCode ??= link.QrCode;
        attempt.AccountName ??= link.AccountName; attempt.BankBin ??= link.BankBin;
        attempt.UpdatedAt = DateTime.UtcNow;
        if (link.Status == "PAID" && link.AmountPaid == attempt.Amount)
        {
            await Paid(invoice, attempt, link.Reference ?? "PAYOS:" + link.Id, ct);
            return;
        }
        if (link.AmountPaid > 0 || link.Status == "PAID") { await Review(attempt, "AmountMismatch", ct); return; }
        if (link.Status is "CANCELLED" or "EXPIRED")
        {
            attempt.Status = link.Status == "EXPIRED" ? "Expired" : "Cancelled"; attempt.IsActive = false;
        }
        else if (attempt.Status != "NeedsReview") attempt.Status = "Pending";
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> Webhook(JsonElement root, CancellationToken ct)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("data", out var data) ||
            !PayOsSignature.Verify(data, PayOsGateway.Text(root, "signature"), options.ChecksumKey))
            throw new PaymentRuleException("INVALID_WEBHOOK_SIGNATURE", "Chữ ký webhook không hợp lệ.");
        if (PayOsGateway.Text(root, "code") != "00" || !root.TryGetProperty("success", out var success) ||
            success.ValueKind != JsonValueKind.True || PayOsGateway.Text(data, "code") != "00") return false;
        if (!data.TryGetProperty("orderCode", out var order) || order.ValueKind != JsonValueKind.Number || !order.TryGetInt64(out var orderCode))
            throw new ArgumentException("Webhook thiếu mã đơn hợp lệ.");
        var attempt = await db.PayOsPaymentAttempts.Include(x => x.Invoice).ThenInclude(x => x.Organization)
            .SingleOrDefaultAsync(x => x.OrderCode == orderCode, ct);
        if (attempt == null) return false; // Signed registration test or unrelated order: never create an invoice.
        var invoice = attempt.Invoice;
        var reference = PayOsGateway.Text(data, "reference");
        var linkId = PayOsGateway.Text(data, "paymentLinkId");
        if (invoice.PaymentMode != "PayOS" || invoice.Organization.OwnerAccountId != attempt.ChannelOwnerAccountId ||
            attempt.ChannelOwnerAccountId != options.DemoOwnerAccountId ||
            PayOsGateway.Text(data, "currency") != "VND" ||
            string.IsNullOrWhiteSpace(linkId) || linkId.Length > 100 ||
            attempt.PaymentLinkId != null && attempt.PaymentLinkId != linkId ||
            string.IsNullOrWhiteSpace(reference) || reference.Length > 100)
        {
            logger?.LogWarning("PayOS webhook identity mismatch. InvoiceId={InvoiceId} OrderCode={OrderCode} ModeMatches={ModeMatches} OwnerMatches={OwnerMatches} CurrencyMatches={CurrencyMatches} LinkMatches={LinkMatches} ReferenceValid={ReferenceValid}",
                invoice.Id, attempt.OrderCode, invoice.PaymentMode == "PayOS",
                invoice.Organization.OwnerAccountId == attempt.ChannelOwnerAccountId && attempt.ChannelOwnerAccountId == options.DemoOwnerAccountId,
                PayOsGateway.Text(data, "currency") == "VND",
                !string.IsNullOrWhiteSpace(linkId) && linkId.Length <= 100 && (attempt.PaymentLinkId == null || attempt.PaymentLinkId == linkId),
                !string.IsNullOrWhiteSpace(reference) && reference.Length <= 100);
            await Review(attempt, "WebhookIdentityMismatch", ct); return true;
        }
        if (!data.TryGetProperty("amount", out var amount) || amount.ValueKind != JsonValueKind.Number || !amount.TryGetDecimal(out var received) ||
            received != attempt.Amount || invoice.TotalAmount != attempt.Amount)
        { await Review(attempt, "AmountMismatch", ct); return true; }
        if (!WebhookAccountMatches(data, attempt))
        {
            // Some VA channels send only the underlying bank account, leaving virtualAccountNumber empty.
            // A signed webhook alone cannot bind that bank account back to this particular VA.
            // Verify the saved order/link and the bank transaction through the signed provider GET response.
            if (IsVirtualAccount(attempt.AccountNumber) &&
                PayOsGateway.Text(data, "accountNumber") == options.ExpectedAccountNumber.Trim() &&
                string.IsNullOrWhiteSpace(PayOsGateway.Text(data, "virtualAccountNumber")) && attempt.PaymentLinkId != null)
            {
                var verified = await gateway.GetAsync(attempt.OrderCode, ct);
                if (verified == null || verified.Status == "PENDING")
                    throw new PaymentProviderException("PAYOS_PAYMENT_NOT_VERIFIED"); // Retry; do not permanently block on provider lag.
                if (HasVerifiedBankTransactions(invoice, attempt, verified) &&
                    verified.Transactions.Any(x => x.Reference == reference && x.Amount == received))
                {
                    await Paid(invoice, attempt, reference!, ct);
                    return true;
                }
            }
            logger?.LogWarning("PayOS webhook account mismatch. InvoiceId={InvoiceId} OrderCode={OrderCode} BankAccountMatches={BankAccountMatches} VirtualAccountPresent={VirtualAccountPresent}",
                invoice.Id, attempt.OrderCode, PayOsGateway.Text(data, "accountNumber") == options.ExpectedAccountNumber.Trim(),
                !string.IsNullOrWhiteSpace(PayOsGateway.Text(data, "virtualAccountNumber")));
            await Review(attempt, "WebhookIdentityMismatch", ct);
            return true;
        }
        attempt.PaymentLinkId ??= linkId;
        await Paid(invoice, attempt, reference, ct);
        return true;
    }

    private bool IsVirtualAccount(string? account) =>
        options.VirtualAccountPrefix.Length >= 3 && account is { Length: >= 6 and <= 50 } &&
        account.StartsWith(options.VirtualAccountPrefix, StringComparison.Ordinal) && account.All(char.IsAsciiLetterOrDigit);

    private bool CanBindVirtualAccount(PayOsPaymentAttempt attempt, PayOsLink link) =>
        attempt.PaymentLinkId == null && attempt.AccountNumber == options.ExpectedAccountNumber.Trim() &&
        !string.IsNullOrWhiteSpace(options.ExpectedBankBin) && link.BankBin == options.ExpectedBankBin &&
        IsVirtualAccount(link.AccountNumber); // Gateway verified HMAC for this configured channel before returning.

    private bool HasVerifiedBankTransactions(Invoice invoice, PayOsPaymentAttempt attempt, PayOsLink link) =>
        invoice.PaymentMode == "PayOS" && invoice.Organization.OwnerAccountId == attempt.ChannelOwnerAccountId &&
        attempt.ChannelOwnerAccountId == options.DemoOwnerAccountId && IsVirtualAccount(attempt.AccountNumber) &&
        !string.IsNullOrWhiteSpace(options.ExpectedBankBin) && attempt.BankBin == options.ExpectedBankBin &&
        !string.IsNullOrWhiteSpace(attempt.PaymentLinkId) && link.Id == attempt.PaymentLinkId &&
        link.OrderCode == attempt.OrderCode && link.Amount == attempt.Amount && invoice.TotalAmount == attempt.Amount &&
        link.Status == "PAID" && link.AmountPaid == attempt.Amount &&
        (link.AccountNumber == null || link.AccountNumber == attempt.AccountNumber) &&
        link.Transactions.Count > 0 && link.Transactions.All(x => x.Amount > 0 &&
            x.AccountNumber == options.ExpectedAccountNumber.Trim() &&
            (string.IsNullOrWhiteSpace(x.VirtualAccountNumber) || x.VirtualAccountNumber == attempt.AccountNumber) &&
            !string.IsNullOrWhiteSpace(x.Reference) && x.Reference.Length <= 100) &&
        link.Transactions.Select(x => x.Reference).Distinct(StringComparer.Ordinal).Count() == link.Transactions.Count &&
        link.Transactions.Sum(x => x.Amount) == link.AmountPaid;

    private bool WebhookAccountMatches(JsonElement data, PayOsPaymentAttempt attempt)
    {
        var account = PayOsGateway.Text(data, "accountNumber");
        var virtualAccount = PayOsGateway.Text(data, "virtualAccountNumber");
        if (account == attempt.AccountNumber)
            return string.IsNullOrWhiteSpace(virtualAccount) || virtualAccount == attempt.AccountNumber ||
                (account == options.ExpectedAccountNumber.Trim() && !string.IsNullOrWhiteSpace(options.ExpectedBankBin) && IsVirtualAccount(virtualAccount));
        // A VA webhook may report the underlying bank account plus the VA used by the payer.
        // Both must match the explicitly configured channel and the saved checkout account.
        return IsVirtualAccount(attempt.AccountNumber) &&
            account == options.ExpectedAccountNumber.Trim() && virtualAccount == attempt.AccountNumber;
    }

    private async Task Paid(Invoice invoice, PayOsPaymentAttempt attempt, string reference, CancellationToken ct)
    {
        if (invoice.Status == "Paid")
        {
            if (attempt.Status == "Paid" && (attempt.ProviderReference == reference || reference == "PAYOS:" + attempt.PaymentLinkId)) return;
            if (attempt.Status == "Paid" && attempt.ProviderReference == "PAYOS:" + attempt.PaymentLinkId)
            {
                attempt.ProviderReference = reference; invoice.PaymentReference = reference;
                var receipt = await db.PaymentReceipts.SingleAsync(x => x.InvoiceId == invoice.Id, ct);
                receipt.ReferenceCode = reference;
                await db.SaveChangesAsync(ct); return;
            }
            await Review(attempt, "InvoiceAlreadyPaid", ct); return;
        }
        if (invoice.TotalAmount != attempt.Amount || reference.Length > 100)
        { await Review(attempt, "AmountMismatch", ct); return; }
        if (invoice.Status is not ("Unpaid" or "AwaitingConfirmation"))
        { await Review(attempt, "InvoiceNotPayable", ct); return; }
        attempt.Status = "Paid"; attempt.IsActive = false; attempt.ProviderReference = reference;
        attempt.ReviewReason = null; attempt.UpdatedAt = DateTime.UtcNow;
        var otherLinks = await db.PayOsPaymentAttempts.Where(x => x.InvoiceId == invoice.Id && x.Id != attempt.Id && x.IsActive).ToListAsync(ct);
        foreach (var other in otherLinks) { other.Status = "CancellationPending"; other.UpdatedAt = DateTime.UtcNow; }
        await settlement.SettleAsync(invoice, "PayOS", null, reference, "PayOS đã xác nhận giao dịch.", ct);
    }

    private async Task CancelOutstanding(Invoice invoice, CancellationToken ct)
    {
        var links = await db.PayOsPaymentAttempts.Where(x => x.InvoiceId == invoice.Id && x.IsActive).ToListAsync(ct);
        foreach (var attempt in links)
        {
            var remote = await gateway.CancelAsync(attempt.OrderCode, ct);
            if (remote.OrderCode != attempt.OrderCode || attempt.PaymentLinkId != null && remote.Id != attempt.PaymentLinkId)
                throw new PaymentProviderException("PAYOS_CANCEL_IDENTITY_MISMATCH");
            if (remote.Status is "CANCELLED" or "EXPIRED")
            {
                attempt.IsActive = false; attempt.Status = remote.Status == "CANCELLED" ? "Cancelled" : "Expired";
            }
            else if (remote.Status == "PAID")
            {
                attempt.IsActive = false; attempt.Status = "NeedsReview"; attempt.ReviewReason = "InvoiceAlreadyPaid";
            }
            attempt.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task Review(PayOsPaymentAttempt attempt, string reason, CancellationToken ct)
    {
        logger?.LogWarning("PayOS payment requires review. InvoiceId={InvoiceId} OrderCode={OrderCode} Reason={Reason}",
            attempt.InvoiceId, attempt.OrderCode, reason);
        if (attempt.Status != "Paid") attempt.Status = "NeedsReview";
        attempt.ReviewReason = reason; attempt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<InvoicePaymentStatusDto> State(Invoice invoice, CancellationToken ct)
    {
        var attempt = await db.PayOsPaymentAttempts.AsNoTracking().Where(x => x.InvoiceId == invoice.Id)
            .OrderByDescending(x => x.Status == "NeedsReview").ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var payment = attempt == null ? null : new PaymentLinkDto(attempt.Id, attempt.OrderCode, attempt.Status, attempt.PaymentLinkId,
            invoice.Status != "Paid" && attempt.Status == "Pending" ? attempt.CheckoutUrl : null,
            invoice.Status != "Paid" && attempt.Status == "Pending" ? attempt.QrCode : null,
            attempt.AccountNumber, attempt.AccountName, attempt.BankBin, DateTime.SpecifyKind(attempt.ExpiresAt, DateTimeKind.Utc), attempt.ReviewReason);
        return new(invoice.Id, invoice.PaymentMode, invoice.Status ?? "Unpaid", invoice.TotalAmount, invoice.PaymentSource,
            invoice.PaidAt.HasValue ? DateTime.SpecifyKind(invoice.PaidAt.Value, DateTimeKind.Utc) : null, payment);
    }

    private static string SafeCheckout(string? url, string id)
    {
        var checkout = url ?? "https://pay.payos.vn/web/" + Uri.EscapeDataString(id);
        if (!Uri.TryCreate(checkout, UriKind.Absolute, out var parsed) || parsed.Scheme != "https" ||
            parsed.Host != "pay.payos.vn" || !string.IsNullOrEmpty(parsed.UserInfo))
            throw new PaymentProviderException("PAYOS_INVALID_CHECKOUT_URL");
        return checkout;
    }
}
