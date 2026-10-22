using System.Net;
using System.Text;
using System.Text.Json;
using NovaStay.Infrastructure.Payments;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class PayOsGatewayTests
{
    [Fact]
    public void Signature_MatchesOfficialWebhookExample_AndRejectsTampering()
    {
        var data = JsonSerializer.SerializeToElement(new
        {
            orderCode = 123, amount = 3000, description = "VQRIO123", accountNumber = "12345678", reference = "TF230204212323",
            transactionDateTime = "2023-02-04 18:25:00", currency = "VND", paymentLinkId = "124c33293c43417ab7879e14c8d9eb18",
            code = "00", desc = "Thành công", counterAccountBankId = "", counterAccountBankName = "", counterAccountName = "",
            counterAccountNumber = "", virtualAccountName = "", virtualAccountNumber = ""
        });
        // Public documentation fixture, split to avoid distributing a deployable-looking key literal.
        var exampleKey = string.Concat("1a54716c", "8f0efb27", "44fb28b6", "e38b25da", "7f67a925", "d98bc1c1", "8bd8faae", "cadd7675");
        const string exampleSignature = "412e915d2871504ed31be63c8f62a149a4410d34c4c42affc9006ef9917eaa03";
        Assert.True(PayOsSignature.Verify(data, exampleSignature, exampleKey));
        Assert.False(PayOsSignature.Verify(data, new string('0', 64), exampleKey));
        Assert.False(PayOsSignature.Verify(data, exampleSignature, ""));
        using var duplicate = JsonDocument.Parse("{\"amount\":1,\"amount\":2}");
        Assert.False(PayOsSignature.Verify(duplicate.RootElement, exampleSignature, exampleKey));
    }

    [Fact]
    public async Task Gateway_SignsCreationRequest_AndChecksProviderResponseSignature()
    {
        var options = new PayOsOptions { ClientId = "test-client", ApiKey = "test-api", ChecksumKey = new string('x', 64), ReturnUrl = "https://example.invalid/return", CancelUrl = "https://example.invalid/cancel" };
        var handler = new ProviderHandler(options);
        using var client = new HttpClient(handler);
        var gateway = new PayOsGateway(client, options);
        var link = await gateway.CreateAsync(123, 64000, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(30), default);
        Assert.Equal(64000, link.Amount); Assert.Equal("PENDING", link.Status);
        Assert.Equal("api-merchant.payos.vn", handler.Host);
        Assert.True(handler.ValidRequestSignature);
        handler.CorruptSignature = true;
        await Assert.ThrowsAsync<PaymentProviderException>(() => gateway.CreateAsync(124, 64000, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(30), default));
    }

    private sealed class ProviderHandler(PayOsOptions options) : HttpMessageHandler
    {
        public string? Host { get; private set; }
        public bool ValidRequestSignature { get; private set; }
        public bool CorruptSignature { get; set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Host = request.RequestUri!.Host;
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            var value = body.RootElement;
            var signed = JsonSerializer.SerializeToElement(new
            {
                amount = value.GetProperty("amount").GetInt32(), cancelUrl = value.GetProperty("cancelUrl").GetString(),
                description = value.GetProperty("description").GetString(), orderCode = value.GetProperty("orderCode").GetInt64(),
                returnUrl = value.GetProperty("returnUrl").GetString()
            });
            ValidRequestSignature = PayOsSignature.Verify(signed, value.GetProperty("signature").GetString(), options.ChecksumKey);
            Assert.True(value.GetProperty("description").GetString()!.Length <= 9);
            var data = JsonSerializer.SerializeToElement(new { orderCode = value.GetProperty("orderCode").GetInt64(), amount = 64000, paymentLinkId = "test-link", status = "PENDING", checkoutUrl = "https://pay.payos.vn/web/test-link", qrCode = "test-qr", accountNumber = "00123456" });
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { code = "00", data, signature = CorruptSignature ? new string('0', 64) : PayOsSignature.Sign(data, options.ChecksumKey) }), Encoding.UTF8, "application/json")
            };
        }
    }
}
