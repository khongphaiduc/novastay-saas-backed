using NovaStay.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace NovaStay.Infrastructure.Payments;

public sealed class PayOsOptions
{
    public bool Enabled { get; set; }
    public Guid DemoOwnerAccountId { get; set; } = Guid.Parse("5c695542-8920-4c39-a470-62d1452a2d2e");
    public string ClientId { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ChecksumKey { get; set; } = "";
    public string ExpectedAccountNumber { get; set; } = "";
    public string VirtualAccountPrefix { get; set; } = "";
    public string ExpectedBankBin { get; set; } = "";
    public string ReturnUrl { get; set; } = "";
    public string CancelUrl { get; set; } = "";
    public int LinkLifetimeMinutes { get; set; } = 30;

    public static PayOsOptions FromConfiguration(IConfiguration config)
    {
        var value = new PayOsOptions
        {
            Enabled = bool.TryParse(config["PayOS:Enabled"], out var enabled) && enabled,
            ClientId = config["PayOS:ClientId"] ?? "", ApiKey = config["PayOS:ApiKey"] ?? "",
            ChecksumKey = config["PayOS:ChecksumKey"] ?? "", ExpectedAccountNumber = config["PayOS:ExpectedAccountNumber"] ?? "",
            VirtualAccountPrefix = config["PayOS:VirtualAccountPrefix"]?.Trim() ?? "",
            ExpectedBankBin = config["PayOS:ExpectedBankBin"]?.Trim() ?? "",
            ReturnUrl = config["PayOS:ReturnUrl"] ?? "", CancelUrl = config["PayOS:CancelUrl"] ?? "",
            LinkLifetimeMinutes = int.TryParse(config["PayOS:LinkLifetimeMinutes"], out var minutes) ? minutes : 30
        };
        if (config["PayOS:DemoOwnerAccountId"] is { } configuredOwner)
            value.DemoOwnerAccountId = Guid.TryParse(configuredOwner, out var owner) ? owner : Guid.Empty;
        return value;
    }

    public bool IsConfigured => Enabled && DemoOwnerAccountId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ChecksumKey) &&
        !string.IsNullOrWhiteSpace(ExpectedAccountNumber) && ValidUrl(ReturnUrl) && ValidUrl(CancelUrl) &&
        LinkLifetimeMinutes is >= 5 and <= 1440;

    private static bool ValidUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment) &&
        (uri.Scheme == "https" || uri.Scheme == "http" && uri.IsLoopback);
}

public sealed class PaymentAccess(PayOsOptions options)
{
    public PaymentCapabilitiesDto Capabilities(Guid ownerId)
    {
        var demo = ownerId == options.DemoOwnerAccountId;
        return new(demo, demo && options.IsConfigured, !demo, demo && !options.IsConfigured,
            !demo ? "PRO_UPGRADE_REQUIRED" : !options.IsConfigured ? "PAYOS_CONFIGURATION_REQUIRED" : "DEMO_ENABLED",
            demo && options.IsConfigured ? ["Manual", "PayOS"] : ["Manual"]);
    }

    public void EnsureReady(Guid ownerId)
    {
        if (ownerId != options.DemoOwnerAccountId)
            throw new PaymentRuleException("PRO_UPGRADE_REQUIRED", "Tính năng tự động chỉ đang mở cho tài khoản demo. Vui lòng đăng ký nâng cấp PRO.");
        if (!options.IsConfigured)
            throw new PaymentRuleException("PAYOS_CONFIGURATION_REQUIRED", "Kênh PayOS demo chưa được cấu hình đầy đủ trên server.");
    }
}

public sealed class PaymentRuleException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}

public sealed class PaymentProviderException(string code) : Exception("Chưa thể đồng bộ với PayOS. Vui lòng thử lại sau.")
{
    public string Code { get; } = code;
}
