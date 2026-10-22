using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NovaStay.Infrastructure.Payments;

public static class PayOsSignature
{
    private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Sign(JsonElement data, string key)
    {
        if (data.ValueKind != JsonValueKind.Object) throw new ArgumentException("Invalid payment data.");
        var properties = data.EnumerateObject().ToList();
        if (properties.Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != properties.Count)
            throw new ArgumentException("Duplicate payment fields.");
        var canonical = string.Join("&", properties.OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => x.Name + "=" + Value(x.Value)));
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    public static bool Verify(JsonElement data, string? signature, string key)
    {
        if (string.IsNullOrWhiteSpace(key) || signature?.Length != 64) return false;
        try
        {
            return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(Sign(data, key)), Convert.FromHexString(signature));
        }
        catch (Exception e) when (e is FormatException or ArgumentException) { return false; }
    }

    private static string Value(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => "",
        JsonValueKind.String => value.GetString() is "null" or "undefined" ? "" : value.GetString() ?? "",
        JsonValueKind.Array or JsonValueKind.Object => JsonSerializer.Serialize(Sorted(value), JsonOptions),
        _ => value.GetRawText()
    };

    private static object? Sorted(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => value.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal).ToDictionary(x => x.Name, x => Sorted(x.Value)),
        JsonValueKind.Array => value.EnumerateArray().Select(Sorted).ToArray(),
        _ => value
    };
}
