using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace NovaStay.API.Security;

public static class ApiRateLimits
{
    public const string Login = "auth-login", Register = "auth-register", Reset = "auth-reset", Refresh = "auth-refresh",
        ChangePassword = "auth-password", PaymentLink = "payment-link", Reconcile = "payment-reconcile",
        BankAccount = "bank-account", Notice = "send-notice", Invitation = "invitation", Hub = "payment-hub", RoomOccupancy = "room-occupancy";

    public static IServiceCollection AddSensitiveEndpointProtection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SensitiveRequestGate>();
        services.AddProblemDetails();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            // Keep loopback defaults. Never trust arbitrary client-supplied X-Forwarded-For.
            foreach (var proxy in configuration.GetSection("ReverseProxy:KnownProxies").GetChildren())
            {
                if (!IPAddress.TryParse(proxy.Value, out var address)) throw new InvalidOperationException("Invalid ReverseProxy:KnownProxies address.");
                options.KnownProxies.Add(address);
            }
        });
        services.AddRateLimiter(options =>
        {
            var windows = new Dictionary<string, int>();
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                var name = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : name != null && windows.TryGetValue(name, out var window) ? window : 60;
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ApiRateLimits))
                    .LogWarning("Sensitive endpoint rate limited. Policy={Policy} TraceId={TraceId} RetryAfterSeconds={RetryAfterSeconds}",
                        name, context.HttpContext.TraceIdentifier, seconds);
                await Reject(context.HttpContext, seconds, ct);
            };
            void Policy(string name, int permits, int seconds, bool authenticated = false)
            {
                var configName = name.Replace("-", "");
                permits = configuration.GetValue<int?>($"Security:RateLimits:{configName}:PermitLimit") ?? permits;
                seconds = configuration.GetValue<int?>($"Security:RateLimits:{configName}:WindowSeconds") ?? seconds;
                if (permits is < 1 or > 100000 || seconds is < 1 or > 86400)
                    throw new InvalidOperationException("Invalid sensitive endpoint rate limit configuration.");
                windows[name] = seconds;
                options.AddPolicy(name, context => RateLimitPartition.GetSlidingWindowLimiter(
                    (authenticated ? UserOrIp(context) : "ip:" + ClientIp(context)), _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = permits, Window = TimeSpan.FromSeconds(seconds), SegmentsPerWindow = Math.Min(6, seconds), QueueLimit = 0, AutoReplenishment = true }));
            }
            Policy(Login, 30, 60); Policy(Register, 10, 3600); Policy(Reset, 5, 900); Policy(Refresh, 60, 60);
            Policy(ChangePassword, 5, 900, true); Policy(PaymentLink, 10, 60, true); Policy(Reconcile, 6, 60, true);
            Policy(BankAccount, 5, 60, true); Policy(Notice, 10, 600, true); Policy(Invitation, 20, 60, true);
            Policy(Hub, 30, 60, true);
            Policy(RoomOccupancy, 20, 60, true);
        });
        return services;
    }

    public static string ClientIp(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address?.IsIPv4MappedToIPv6 == true) address = address.MapToIPv4();
        return address?.ToString() ?? "unknown";
    }

    public static string UserOrIp(HttpContext context) => context.User.Identity?.IsAuthenticated == true
        && Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)
            ? "user:" + accountId.ToString("D") : "ip:" + ClientIp(context);

    public static object Error(int seconds) => new { code = "RATE_LIMIT_EXCEEDED",
        message = $"Bạn thao tác quá nhanh. Vui lòng thử lại sau {seconds} giây.", retryAfterSeconds = seconds };

    public static async Task Reject(HttpContext context, int seconds, CancellationToken ct)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        await context.Response.WriteAsJsonAsync(Error(seconds), ct);
    }
}
