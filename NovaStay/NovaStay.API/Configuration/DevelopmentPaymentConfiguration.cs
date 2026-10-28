using Microsoft.Extensions.Configuration;

namespace NovaStay.API.Configuration;

public static class DevelopmentPaymentConfiguration
{
    public static void Apply(IConfigurationBuilder configuration, bool isDevelopment,
        IEnumerable<KeyValuePair<string, string>> localValues)
    {
        if (!isDevelopment) return;
        var local = localValues.ToArray();
        var workers = local.FirstOrDefault(x => x.Key.Equals("Payments__BackgroundWorkersEnabled", StringComparison.OrdinalIgnoreCase));
        if (workers.Key != null)
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            { ["Payments:BackgroundWorkersEnabled"] = workers.Value });
        // IDEs inherit stale environment variables until their parent process restarts.
        // Use one complete local PayOS configuration instead of mixing two channels.
        var values = local
            .Where(x => x.Key.StartsWith("PayOS__", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key.Replace("__", ":"), x => (string?)x.Value, StringComparer.OrdinalIgnoreCase);
        if (values.Count == 0) return;
        foreach (var key in new[] { "Enabled", "DemoOwnerAccountId", "ClientId", "ApiKey", "ChecksumKey",
                     "ExpectedAccountNumber", "VirtualAccountPrefix", "ExpectedBankBin", "ReturnUrl", "CancelUrl", "LinkLifetimeMinutes" })
            values.TryAdd("PayOS:" + key, null);
        configuration.AddInMemoryCollection(values);
    }
}
