using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NovaStay.Infrastructure.Payments;

public sealed record PayOsTransaction(string? AccountNumber, decimal Amount, string? Reference, string? VirtualAccountNumber);

public sealed record PayOsLink(long OrderCode, decimal Amount, string Id, string Status, decimal AmountPaid,
    string? CheckoutUrl, string? QrCode, string? AccountNumber, string? AccountName, string? BankBin, string? Reference)
{
    public IReadOnlyList<PayOsTransaction> Transactions { get; init; } = [];
}

public interface IPayOsGateway
{
    Task<PayOsLink> CreateAsync(long orderCode, int amount, Guid invoiceId, DateTime expiresAt, CancellationToken ct);
    Task<PayOsLink?> GetAsync(long orderCode, CancellationToken ct);
    Task<PayOsLink> CancelAsync(long orderCode, CancellationToken ct);
}

public sealed class PayOsGateway(HttpClient client, PayOsOptions options) : IPayOsGateway
{
    public async Task<PayOsLink> CreateAsync(long orderCode, int amount, Guid invoiceId, DateTime expiresAt, CancellationToken ct)
    {
        var returnUrl = Redirect(options.ReturnUrl, invoiceId);
        var cancelUrl = Redirect(options.CancelUrl, invoiceId);
        var description = "HD" + invoiceId.ToString("N")[..7].ToUpperInvariant(); // 9 characters for supported bank limits.
        var signedData = JsonSerializer.SerializeToElement(new { amount, cancelUrl, description, orderCode, returnUrl });
        using var request = Request(HttpMethod.Post, "v2/payment-requests");
        request.Content = JsonContent.Create(new
        {
            orderCode, amount, description, cancelUrl, returnUrl,
            expiredAt = new DateTimeOffset(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            signature = PayOsSignature.Sign(signedData, options.ChecksumKey)
        });
        var data = await Send(request, ct) ?? throw new PaymentProviderException("PAYOS_CREATE_FAILED");
        return Parse(data);
    }

    public async Task<PayOsLink?> GetAsync(long orderCode, CancellationToken ct)
    {
        using var request = Request(HttpMethod.Get, $"v2/payment-requests/{orderCode}");
        var data = await Send(request, ct);
        return data.HasValue ? Parse(data.Value) : null;
    }

    public async Task<PayOsLink> CancelAsync(long orderCode, CancellationToken ct)
    {
        using var request = Request(HttpMethod.Post, $"v2/payment-requests/{orderCode}/cancel");
        request.Content = JsonContent.Create(new { cancellationReason = "Invoice already settled" });
        return Parse(await Send(request, ct) ?? throw new PaymentProviderException("PAYOS_CANCEL_FAILED"));
    }

    private HttpRequestMessage Request(HttpMethod method, string path)
    {
        // Fixed official host: no browser-provided URLs or channel keys.
        var request = new HttpRequestMessage(method, "https://api-merchant.payos.vn/" + path);
        request.Headers.Add("x-client-id", options.ClientId);
        request.Headers.Add("x-api-key", options.ApiKey);
        return request;
    }

    private async Task<JsonElement?> Send(HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            using var response = await client.SendAsync(request, ct);
            if (request.Method == HttpMethod.Get && response.StatusCode == HttpStatusCode.NotFound) return null;
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = document.RootElement;
            var code = Text(root, "code");
            if (!response.IsSuccessStatusCode || code != "00") throw new PaymentProviderException("PAYOS_REQUEST_FAILED");
            var data = root.GetProperty("data");
            if (!PayOsSignature.Verify(data, Text(root, "signature"), options.ChecksumKey))
                throw new PaymentProviderException("PAYOS_INVALID_RESPONSE_SIGNATURE");
            return data.Clone();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        { throw new PaymentProviderException("PAYOS_UNAVAILABLE"); }
    }

    private static PayOsLink Parse(JsonElement data)
    {
        var receipts = new List<PayOsTransaction>();
        if (data.TryGetProperty("transactions", out var transactions) && transactions.ValueKind == JsonValueKind.Array)
            foreach (var transaction in transactions.EnumerateArray())
                receipts.Add(new(Text(transaction, "accountNumber"),
                    transaction.TryGetProperty("amount", out var amount) && amount.TryGetDecimal(out var value) ? value : 0,
                    Text(transaction, "reference"), Text(transaction, "virtualAccountNumber")));
        var reference = receipts.Select(x => x.Reference).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return new(data.GetProperty("orderCode").GetInt64(), data.GetProperty("amount").GetDecimal(),
            Text(data, "paymentLinkId") ?? Text(data, "id") ?? "", Text(data, "status") ?? "PENDING",
            data.TryGetProperty("amountPaid", out var paid) ? paid.GetDecimal() : 0,
            Text(data, "checkoutUrl"), Text(data, "qrCode"), Text(data, "accountNumber"), Text(data, "accountName"), Text(data, "bin"), reference)
        { Transactions = receipts };
    }

    internal static string? Text(JsonElement data, string key) => data.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static string Redirect(string url, Guid id) => url + (url.Contains('?') ? "&" : "?") + "invoiceId=" + id;
}
