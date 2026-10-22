using System.Text.Json;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.Payments;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed partial class BillingServiceTests
{
    private PayOsOptions TestOptions() => new()
    {
        Enabled = true, DemoOwnerAccountId = owner, ClientId = "test-only-client", ApiKey = "test-only-api",
        ChecksumKey = new string('x', 64), ExpectedAccountNumber = "00123456",
        ReturnUrl = "https://example.invalid/payment-return", CancelUrl = "https://example.invalid/payment-cancel"
    };

    private PaymentProcessingService Payments(PayOsOptions options, FakePayOs gateway) =>
        new(db, options, new PaymentAccess(options), gateway, new InvoiceSettlementService(db));

    private async Task<BillDto> PayOsInvoice(PayOsOptions options)
    {
        var request = Request("rent"); request.PaymentMode = "PayOS";
        return Assert.Single(await new BillingService(db, new PaymentAccess(options)).Issue(owner, request));
    }

    private static JsonElement Webhook(PayOsOptions options, PaymentLinkDto link, decimal amount, string? account = null, string? reference = null, string? virtualAccount = null)
    {
        var data = JsonSerializer.SerializeToElement(new
        {
            orderCode = link.OrderCode, amount, accountNumber = account ?? options.ExpectedAccountNumber,
            paymentLinkId = link.PaymentLinkId, currency = "VND", code = "00", reference = reference ?? "TX" + link.OrderCode,
            description = "TEST", transactionDateTime = "2026-10-22 10:00:00"
            , virtualAccountNumber = virtualAccount ?? ""
        });
        return JsonSerializer.SerializeToElement(new { code = "00", success = true, data, signature = PayOsSignature.Sign(data, options.ChecksumKey) });
    }

    [Fact]
    public async Task PayOsWebhook_SettlesExactlyOnceAndQueuesBothRecipients()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options);
        var invoice = await PayOsInvoice(options);
        Assert.Equal("PayOS", invoice.PaymentMode); Assert.Null(invoice.BankAccount);
        var payments = Payments(options, gateway);
        var pending = await payments.CreateLink(residentAccount, invoice.Id, default);
        Assert.Equal("Pending", pending.Payment!.Status);
        var webhook = Webhook(options, pending.Payment, invoice.TotalAmount);
        Assert.True(await payments.Webhook(webhook, default));
        Assert.True(await payments.Webhook(webhook, default));
        var state = await payments.Status(residentAccount, invoice.Id, default);
        Assert.Equal("Paid", state.InvoiceStatus); Assert.Equal("PayOS", state.PaymentSource);
        Assert.Null(state.Payment!.CheckoutUrl); Assert.Null(state.Payment.QrCode);
        Assert.Single(db.PaymentReceipts); Assert.Single(db.IncomeReceipts);
        Assert.Null((await db.Invoices.SingleAsync()).ConfirmedByAccountId);
        var notification = await db.PaymentNotificationOutbox.SingleAsync();
        Assert.Equal(owner, notification.OwnerAccountId); Assert.Equal(residentAccount, notification.ResidentAccountId);
        Assert.Null(notification.SentAt);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => payments.Status(Guid.NewGuid(), invoice.Id, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => payments.Reconcile(residentAccount, invoice.Id, default));
    }

    [Fact]
    public async Task PayOs_InvalidSignatureCannotConfirm_AndMismatchedAmountRequiresReview()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options); var payments = Payments(options, gateway);
        var invoice = await PayOsInvoice(options);
        var pending = await payments.CreateLink(owner, invoice.Id, default);
        var signed = Webhook(options, pending.Payment!, invoice.TotalAmount);
        var invalid = JsonSerializer.SerializeToElement(new { code = "00", success = true, data = signed.GetProperty("data"), signature = new string('0', 64) });
        var error = await Assert.ThrowsAsync<PaymentRuleException>(() => payments.Webhook(invalid, default));
        Assert.Equal("INVALID_WEBHOOK_SIGNATURE", error.Code);
        Assert.Empty(db.IncomeReceipts);
        await payments.Webhook(Webhook(options, pending.Payment!, 100), default);
        Assert.Empty(db.IncomeReceipts); Assert.Empty(db.PaymentNotificationOutbox);
        var state = await payments.Status(owner, invoice.Id, default);
        Assert.Equal("Unpaid", state.InvoiceStatus); Assert.Equal("NeedsReview", state.Payment!.Status);
        Assert.Equal("AmountMismatch", state.Payment.ReviewReason);
        Assert.Null(state.Payment.QrCode);
        await Assert.ThrowsAsync<PaymentRuleException>(() => payments.CreateLink(owner, invoice.Id, default));
    }

    [Fact]
    public async Task PayOs_WrongRecipientCannotBeSettledByWebhookOrReconciliation()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options); var payments = Payments(options, gateway);
        var invoice = await PayOsInvoice(options); var pending = await payments.CreateLink(owner, invoice.Id, default);
        await payments.Webhook(Webhook(options, pending.Payment!, invoice.TotalAmount, "wrong-account"), default);
        gateway.MarkPaid(pending.Payment!.OrderCode);
        await Assert.ThrowsAsync<PaymentRuleException>(() => payments.Reconcile(owner, invoice.Id, default));
        Assert.Empty(db.IncomeReceipts);
    }

    [Fact]
    public async Task PayOs_CreateTimeoutRecoversSameOrder_AndReusesPendingLink()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options) { TimeoutOnce = true };
        var invoice = await PayOsInvoice(options); var payments = Payments(options, gateway);
        var first = await payments.CreateLink(owner, invoice.Id, default);
        var second = await payments.CreateLink(residentAccount, invoice.Id, default);
        Assert.Equal(first.Payment!.OrderCode, second.Payment!.OrderCode);
        Assert.Equal(1, gateway.CreateCalls); Assert.Single(gateway.Links); Assert.Single(db.PayOsPaymentAttempts);
        Assert.Empty(db.PaymentReceipts);
    }

    [Fact]
    public async Task PayOs_ReconciliationRecoversMissedWebhook_AndEnrichesReferenceWithoutDuplicateIncome()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options); var payments = Payments(options, gateway);
        var invoice = await PayOsInvoice(options); var pending = await payments.CreateLink(owner, invoice.Id, default);
        gateway.MarkPaid(pending.Payment!.OrderCode);
        var state = await payments.Reconcile(owner, invoice.Id, default);
        Assert.Equal("Paid", state.InvoiceStatus);
        await payments.Webhook(Webhook(options, pending.Payment, invoice.TotalAmount, reference: "BANK-REAL-REFERENCE"), default);
        Assert.Single(db.PaymentReceipts); Assert.Single(db.IncomeReceipts); Assert.Single(db.PaymentNotificationOutbox);
        Assert.Equal("BANK-REAL-REFERENCE", (await db.PaymentReceipts.SingleAsync()).ReferenceCode);
    }

    [Fact]
    public async Task PayOs_LatePaymentForExpiredLinkCancelsReplacementWithoutDoubleSettlement()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options); var payments = Payments(options, gateway);
        var invoice = await PayOsInvoice(options); var first = await payments.CreateLink(owner, invoice.Id, default);
        var old = await db.PayOsPaymentAttempts.SingleAsync(); old.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        gateway.Links[first.Payment!.OrderCode] = gateway.Links[first.Payment.OrderCode] with { Status = "EXPIRED" };
        var replacement = await payments.CreateLink(owner, invoice.Id, default);
        Assert.NotEqual(first.Payment.OrderCode, replacement.Payment!.OrderCode);
        await payments.Webhook(Webhook(options, first.Payment, invoice.TotalAmount), default);
        Assert.Single(db.PaymentReceipts);
        Assert.Equal("CancellationPending", (await db.PayOsPaymentAttempts.SingleAsync(x => x.IsActive)).Status);
        await payments.Reconcile(owner, invoice.Id, default);
        Assert.False(await db.PayOsPaymentAttempts.AnyAsync(x => x.IsActive));
        Assert.Single(db.PaymentReceipts);
    }

    [Fact]
    public async Task PayOs_IsGatedOnServer_AndManualConfirmationCannotBypassProvider()
    {
        var options = TestOptions();
        var access = new PaymentAccess(options);
        Assert.True(access.Capabilities(Guid.NewGuid()).RequiresUpgrade);
        var unauthorized = TestOptions(); unauthorized.DemoOwnerAccountId = Guid.NewGuid();
        var request = Request("rent"); request.PaymentMode = "PayOS";
        var error = await Assert.ThrowsAsync<PaymentRuleException>(() => new BillingService(db, new PaymentAccess(unauthorized)).Issue(owner, request));
        Assert.Equal("PRO_UPGRADE_REQUIRED", error.Code); Assert.Empty(db.Invoices);
        options.Enabled = false;
        Assert.False(access.Capabilities(owner).CanUsePayOs);
        await Assert.ThrowsAsync<PaymentRuleException>(() => PayOsInvoice(options));
        options.Enabled = true;
        var invoice = await PayOsInvoice(options);
        await Assert.ThrowsAsync<PaymentRuleException>(() => service.Confirm(owner, invoice.Id, null));
        await Assert.ThrowsAsync<PaymentRuleException>(() => service.Report(residentAccount, invoice.Id));
        Assert.Empty(db.PaymentReceipts);
    }

    [Fact]
    public async Task PaymentOutbox_RetriesNotificationFailureWithStableEventId()
    {
        await SaveBank(); var invoice = Assert.Single(await service.Issue(owner, Request("rent")));
        await service.Confirm(owner, invoice.Id, null);
        var notifier = new FakeNotifier();
        using var provider = new ServiceCollection().AddSingleton(db).AddSingleton<IPaymentNotifier>(notifier).BuildServiceProvider();
        var worker = new PaymentOutboxWorker(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<PaymentOutboxWorker>.Instance);
        await worker.Dispatch(default);
        var item = await db.PaymentNotificationOutbox.SingleAsync();
        Assert.Null(item.SentAt); Assert.Equal(1, item.Attempts); Assert.Single(db.IncomeReceipts);
        item.NextAttemptAt = DateTime.UtcNow.AddSeconds(-1); await db.SaveChangesAsync();
        await worker.Dispatch(default);
        Assert.NotNull(item.SentAt); Assert.Equal(2, item.Attempts);
        Assert.Equal(item.Id, Assert.Single(notifier.Events).EventId);
    }

    [Fact]
    public async Task PayOs_CannotCreateLinkForCancelledInvoice()
    {
        var options = TestOptions(); var gateway = new FakePayOs(options);
        var invoice = await PayOsInvoice(options);
        (await db.Invoices.SingleAsync()).Status = "Cancelled";
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<PaymentRuleException>(() => Payments(options, gateway).CreateLink(owner, invoice.Id, default));
        Assert.Equal("INVOICE_NOT_PAYABLE", error.Code);
        Assert.Equal(0, gateway.CreateCalls); Assert.Empty(db.PayOsPaymentAttempts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PayOs_ConfiguredVirtualAccount_CreatesQrAndAcceptsSignedPayment(bool underlyingBankAccount)
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var gateway = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options);
        var payments = Payments(options, gateway);
        var state = await payments.CreateLink(residentAccount, invoice.Id, default);
        Assert.Equal("Pending", state.Payment!.Status);
        Assert.Equal("VQRTEST001", state.Payment.AccountNumber);
        Assert.Equal("VQRTEST001", (await db.PayOsPaymentAttempts.SingleAsync()).AccountNumber);
        await payments.Webhook(Webhook(options, state.Payment, state.Amount,
            underlyingBankAccount ? options.ExpectedAccountNumber : "VQRTEST001",
            virtualAccount: underlyingBankAccount ? "VQRTEST001" : null), default);
        Assert.Equal("Paid", (await payments.Status(residentAccount, invoice.Id, default)).InvoiceStatus);
        Assert.Single(db.PaymentReceipts);
    }

    [Fact]
    public async Task PayOs_UnconfiguredCheckoutAccount_RemainsBlocked()
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var gateway = new FakePayOs(options) { ResponseAccountNumber = "WRONGACCOUNT001" };
        var invoice = await PayOsInvoice(options);
        var error = await Assert.ThrowsAsync<PaymentRuleException>(() => Payments(options, gateway).CreateLink(owner, invoice.Id, default));
        Assert.Equal("PAYOS_CHANNEL_MISMATCH", error.Code);
        Assert.Equal("NeedsReview", (await db.PayOsPaymentAttempts.SingleAsync()).Status);
        Assert.Empty(db.PaymentReceipts);
    }

    [Theory]
    [InlineData("", "970000")]
    [InlineData("VQR", "970999")]
    public async Task PayOs_VirtualAccountRequiresExplicitPrefixAndMatchingBank(string prefix, string bankBin)
    {
        var options = TestOptions(); options.VirtualAccountPrefix = prefix; options.ExpectedBankBin = bankBin;
        var gateway = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options);
        var error = await Assert.ThrowsAsync<PaymentRuleException>(() => Payments(options, gateway).CreateLink(owner, invoice.Id, default));
        Assert.Equal("PAYOS_CHANNEL_MISMATCH", error.Code);
        Assert.Empty(db.PaymentReceipts);
    }

    [Fact]
    public async Task PayOs_VirtualWebhookCannotSubstituteAnotherBankAccount()
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var gateway = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options); var payments = Payments(options, gateway);
        var state = await payments.CreateLink(owner, invoice.Id, default);
        await payments.Webhook(Webhook(options, state.Payment!, state.Amount, "wrong-bank-account", virtualAccount: "VQRTEST001"), default);
        Assert.Equal("NeedsReview", (await db.PayOsPaymentAttempts.SingleAsync()).Status);
        Assert.Empty(db.PaymentReceipts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PayOs_BankOnlyVaWebhook_RequiresSignedProviderTransaction(bool corruptProviderSignature)
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var fake = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options);
        var state = await Payments(options, fake).CreateLink(owner, invoice.Id, default);
        using var client = new HttpClient(new BankOnlyProviderHandler(options, fake.Links[state.Payment!.OrderCode], corruptProviderSignature));
        var payments = new PaymentProcessingService(db, options, new PaymentAccess(options),
            new PayOsGateway(client, options), new InvoiceSettlementService(db));
        var payload = Webhook(options, state.Payment, state.Amount, reference: "BANK-ONLY-REFERENCE");
        if (corruptProviderSignature)
        {
            await Assert.ThrowsAsync<PaymentProviderException>(() => payments.Webhook(payload, default));
            Assert.Equal("Pending", (await db.PayOsPaymentAttempts.SingleAsync()).Status);
            Assert.Empty(db.PaymentReceipts); Assert.Empty(db.PaymentNotificationOutbox);
            return;
        }
        await payments.Webhook(payload, default);
        await payments.Webhook(payload, default);
        Assert.Equal("Paid", (await payments.Status(owner, invoice.Id, default)).InvoiceStatus);
        Assert.Equal("BANK-ONLY-REFERENCE", (await db.PaymentReceipts.SingleAsync()).ReferenceCode);
        Assert.Single(db.IncomeReceipts); Assert.Single(db.PaymentNotificationOutbox);
    }

    [Fact]
    public async Task PayOs_BankOnlyVaWebhook_ProviderLagRetriesWithoutBlocking_ThenSettles()
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var fake = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options); var payments = Payments(options, fake);
        var state = await payments.CreateLink(owner, invoice.Id, default);
        var payload = Webhook(options, state.Payment!, state.Amount, reference: "BANK-ONLY-REFERENCE");
        var error = await Assert.ThrowsAsync<PaymentProviderException>(() => payments.Webhook(payload, default));
        Assert.Equal("PAYOS_PAYMENT_NOT_VERIFIED", error.Code);
        Assert.Equal("Pending", (await db.PayOsPaymentAttempts.SingleAsync()).Status);
        fake.MarkBankPaid(state.Payment!.OrderCode);
        await payments.Webhook(payload, default);
        Assert.Single(db.PaymentReceipts); Assert.Single(db.PaymentNotificationOutbox);
    }

    [Fact]
    public async Task PayOs_LegacyIdentityReview_CanRecoverOnlyThroughVerifiedBankTransactions()
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var fake = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options); var payments = Payments(options, fake);
        var state = await payments.CreateLink(owner, invoice.Id, default);
        var attempt = await db.PayOsPaymentAttempts.SingleAsync();
        attempt.Status = "NeedsReview"; attempt.ReviewReason = "WebhookIdentityMismatch";
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<PaymentRuleException>(() => payments.Reconcile(owner, invoice.Id, default));
        fake.MarkBankPaid(state.Payment!.OrderCode);
        var result = await payments.Reconcile(owner, invoice.Id, default);
        Assert.Equal("Paid", result.InvoiceStatus); Assert.Null(result.Payment!.ReviewReason);
        await payments.Reconcile(owner, invoice.Id, default);
        Assert.Single(db.PaymentReceipts); Assert.Single(db.IncomeReceipts); Assert.Single(db.PaymentNotificationOutbox);
    }

    [Theory]
    [InlineData("order")]
    [InlineData("link")]
    [InlineData("amount")]
    [InlineData("paid")]
    [InlineData("bank")]
    [InlineData("virtual")]
    [InlineData("reference")]
    [InlineData("missing-transactions")]
    public async Task PayOs_BankOnlyVaWebhook_RejectsUnprovenProviderIdentity(string mismatch)
    {
        var options = TestOptions(); options.VirtualAccountPrefix = "VQR"; options.ExpectedBankBin = "970000";
        var fake = new FakePayOs(options) { ResponseAccountNumber = "VQRTEST001" };
        var invoice = await PayOsInvoice(options); var payments = Payments(options, fake);
        var state = await payments.CreateLink(owner, invoice.Id, default); var code = state.Payment!.OrderCode;
        fake.MarkBankPaid(code); var link = fake.Links[code];
        fake.Links[code] = mismatch switch
        {
            "order" => link with { OrderCode = code + 1 },
            "link" => link with { Id = "another-link" },
            "amount" => link with { Amount = link.Amount + 1 },
            "paid" => link with { AmountPaid = link.AmountPaid - 1 },
            "bank" => link with { Transactions = [link.Transactions[0] with { AccountNumber = "another-bank-account" }] },
            "virtual" => link with { Transactions = [link.Transactions[0] with { VirtualAccountNumber = "VQRWRONG001" }] },
            "reference" => link with { Transactions = [link.Transactions[0] with { Reference = "another-reference" }] },
            _ => link with { Transactions = [] }
        };
        await payments.Webhook(Webhook(options, state.Payment, state.Amount, reference: "BANK-ONLY-REFERENCE"), default);
        Assert.Equal("NeedsReview", (await db.PayOsPaymentAttempts.SingleAsync()).Status);
        Assert.Empty(db.PaymentReceipts); Assert.Empty(db.IncomeReceipts); Assert.Empty(db.PaymentNotificationOutbox);
    }

    private sealed class BankOnlyProviderHandler(PayOsOptions options, PayOsLink link, bool corruptSignature) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            var data = JsonSerializer.SerializeToElement(new
            {
                id = link.Id, orderCode = link.OrderCode, amount = link.Amount, amountPaid = link.Amount, amountRemaining = 0,
                status = "PAID", createdAt = "2026-10-22T09:25:28Z",
                transactions = new[] { new { accountNumber = options.ExpectedAccountNumber, amount = link.Amount,
                    reference = "BANK-ONLY-REFERENCE", virtualAccountNumber = "", virtualAccountName = "",
                    description = "TEST", transactionDateTime = "2026-10-22 16:27:44" } }
            });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { code = "00", data,
                    signature = corruptSignature ? new string('0', 64) : PayOsSignature.Sign(data, options.ChecksumKey) }), Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FakeNotifier : IPaymentNotifier
    {
        private bool fail = true;
        public List<InvoicePaymentUpdated> Events { get; } = [];
        public Task NotifyAsync(Guid ownerAccountId, Guid? residentAccountId, InvoicePaymentUpdated message, CancellationToken ct)
        {
            if (fail) { fail = false; throw new IOException("Simulated hub outage"); }
            Events.Add(message); return Task.CompletedTask;
        }
    }

    private sealed class FakePayOs(PayOsOptions options) : IPayOsGateway
    {
        public Dictionary<long, PayOsLink> Links { get; } = [];
        public int CreateCalls { get; private set; }
        public bool TimeoutOnce { get; set; }
        public string? ResponseAccountNumber { get; set; }
        public Task<PayOsLink> CreateAsync(long orderCode, int amount, Guid invoiceId, DateTime expiresAt, CancellationToken ct)
        {
            CreateCalls++;
            var link = new PayOsLink(orderCode, amount, Guid.NewGuid().ToString("N"), "PENDING", 0,
                "https://pay.payos.vn/web/test", "test-qr-payload", ResponseAccountNumber ?? options.ExpectedAccountNumber, "TEST ACCOUNT", "970000", null);
            Links.TryAdd(orderCode, link);
            if (TimeoutOnce) { TimeoutOnce = false; throw new PaymentProviderException("SIMULATED_TIMEOUT"); }
            return Task.FromResult(Links[orderCode]);
        }
        public Task<PayOsLink?> GetAsync(long orderCode, CancellationToken ct) => Task.FromResult(Links.GetValueOrDefault(orderCode));
        public Task<PayOsLink> CancelAsync(long orderCode, CancellationToken ct)
        {
            var link = Links[orderCode] with { Status = "CANCELLED" }; Links[orderCode] = link; return Task.FromResult(link);
        }
        public void MarkPaid(long code) => Links[code] = Links[code] with { Status = "PAID", AmountPaid = Links[code].Amount };
        public void MarkBankPaid(long code) => Links[code] = Links[code] with
        {
            Status = "PAID", AmountPaid = Links[code].Amount, AccountNumber = null,
            Transactions = [new PayOsTransaction(options.ExpectedAccountNumber, Links[code].Amount, "BANK-ONLY-REFERENCE", "")]
        };
    }
}
