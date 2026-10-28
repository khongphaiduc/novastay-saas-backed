using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NovaStay.API.Controllers;
using NovaStay.API.Hubs;
using NovaStay.API.Security;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.UnitTests.API;

public class SensitiveEndpointIntegrationTests
{
    private sealed class Server : IAsyncDisposable
    {
        public WebApplication App = null!;
        public HttpClient Client = null!;
        public Mock<IResidentAuthService> Resident = new();
        public Mock<IAuthService> Auth = new();
        public static async Task<Server> Start(int loginLimit = 30, bool trustLoopback = true, bool failLogin = false)
        {
            var server = new Server();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ContentRootPath = AppContext.BaseDirectory });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            { ["Security:RateLimits:AuthLogin:PermitLimit"] = loginLimit.ToString(),
              ["Security:RateLimits:BankAccount:PermitLimit"] = "2" });
            builder.Services.AddSensitiveEndpointProtection(builder.Configuration);
            if (!trustLoopback) builder.Services.PostConfigure<ForwardedHeadersOptions>(o =>
            { o.KnownProxies.Clear(); o.KnownNetworks.Clear(); o.KnownProxies.Add(IPAddress.Parse("192.0.2.254")); });
            builder.Services.AddExceptionHandler<SafeApiExceptionHandler>();
            builder.Services.AddSingleton(server.Resident.Object); builder.Services.AddSingleton(server.Auth.Object);
            builder.Services.AddSingleton(Mock.Of<IChangePasswordService>()); builder.Services.AddSingleton(Mock.Of<ILogoutService>());
            builder.Services.AddSingleton(Mock.Of<IProvideAccessToken>());
            builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly).AddApplicationPart(typeof(SecurityFixtureController).Assembly);
            builder.Services.AddSignalR(); builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
                    ValidIssuer = "test", ValidAudience = "test", IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('t', 64))) };
                o.Events = new() { OnMessageReceived = PaymentHubAuthentication.ReadToken };
            });
            server.Resident.Setup(x => x.GoogleLoginAsync(It.IsAny<GoogleLoginResidentRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LoginResidentAccountResponse());
            var login = server.Resident.Setup(x => x.LoginAsync(It.IsAny<LoginResidentAccountRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()));
            if (failLogin) login.ThrowsAsync(new UnauthorizedAccessException("Invalid phone or password."));
            else login.ReturnsAsync(new LoginResidentAccountResponse());
            server.Auth.Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());
            server.App = builder.Build();
            server.App.UseForwardedHeaders(); server.App.UseExceptionHandler(); server.App.UseRouting();
            server.App.UseAuthentication(); server.App.UseRateLimiter(); server.App.UseAuthorization(); server.App.MapControllers();
            server.App.MapPost("/rate-test/bank", () => "saved").RequireAuthorization().RequireRateLimiting(ApiRateLimits.BankAccount);
            server.App.MapPost("/rate-test/webhook", () => "received");
            server.App.MapHub<PaymentsHub>("/hubs/payments").RequireRateLimiting(ApiRateLimits.Hub);
            await server.App.StartAsync();
            var address = server.App.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            server.Client = new HttpClient { BaseAddress = new Uri(address) }; return server;
        }
        public async Task<HttpResponseMessage> Google(string ip)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/resident/google-login")
            { Content = JsonContent.Create(new { idToken = "fixture-token" }) };
            request.Headers.Add("X-Forwarded-For", ip); return await Client.SendAsync(request);
        }
        public static string Token(Guid user) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("test", "test",
            [new Claim(ClaimTypes.NameIdentifier, user.ToString()), new Claim(ClaimTypes.Role, "BusinessOwner")], expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('t', 64))), SecurityAlgorithms.HmacSha256)));
        public async ValueTask DisposeAsync() { Client.Dispose(); await App.StopAsync(); await App.DisposeAsync(); }
    }

    [Fact]
    public async Task TrustedProxySeparatesClients_And429IncludesRetryAfterWithoutCallingAction()
    {
        await using var s = await Server.Start(2);
        Assert.Equal(HttpStatusCode.OK, (await s.Google("192.0.2.1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await s.Google("192.0.2.1")).StatusCode);
        var blocked = await s.Google("192.0.2.1"); Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.NotNull(blocked.Headers.RetryAfter); Assert.Contains("RATE_LIMIT_EXCEEDED", await blocked.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await s.Google("192.0.2.2")).StatusCode);
        s.Resident.Verify(x => x.GoogleLoginAsync(It.IsAny<GoogleLoginResidentRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task UntrustedForwardedHeaderCannotRotateAroundIpLimit()
    {
        await using var s = await Server.Start(2, false);
        await s.Google("192.0.2.1"); await s.Google("192.0.2.2");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await s.Google("192.0.2.3")).StatusCode);
    }

    [Fact]
    public async Task FailedPasswordBudgetIsPerTarget_AndDoesNotBlockOtherPeopleOnSharedWifi()
    {
        await using var s = await Server.Start(failLogin: true);
        for (var n = 0; n < 5; n++) Assert.Equal(HttpStatusCode.Unauthorized,
            (await s.Client.PostAsJsonAsync("/api/auth/resident/login", new { sdt = "0900000001", password = "wrong" })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests,
            (await s.Client.PostAsJsonAsync("/api/auth/resident/login", new { sdt = "0900000001", password = "wrong" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await s.Client.PostAsJsonAsync("/api/auth/resident/login", new { sdt = "0900000002", password = "wrong" })).StatusCode);
    }

    [Fact]
    public async Task ResetTargetCooldownIsNormalized_AndDoesNotRevealMissingAccounts()
    {
        await using var s = await Server.Start();
        var first = await s.Client.PostAsJsonAsync("/api/auth/reset-password", new { email = "Person@Example.invalid" });
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        var blocked = await s.Client.PostAsJsonAsync("/api/auth/reset-password", new { email = " person@example.invalid " });
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode); Assert.NotNull(blocked.Headers.RetryAfter);
        Assert.Equal(HttpStatusCode.Accepted, (await s.Client.PostAsJsonAsync("/api/auth/reset-password", new { email = "other@example.invalid" })).StatusCode);
    }

    [Fact]
    public async Task UserLimitsAndInvoiceCooldown_UseAuthenticatedIdentity()
    {
        await using var s = await Server.Start();
        s.Client.DefaultRequestHeaders.Authorization = new("Bearer", Server.Token(Guid.NewGuid()));
        await s.Client.PostAsync("/rate-test/bank", null); await s.Client.PostAsync("/rate-test/bank", null);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await s.Client.PostAsync("/rate-test/bank", null)).StatusCode);
        s.Client.DefaultRequestHeaders.Authorization = new("Bearer", Server.Token(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.OK, (await s.Client.PostAsync("/rate-test/bank", null)).StatusCode);
        var invoice = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.OK, (await s.Client.PostAsync("/security-fixture/invoices/" + invoice, null)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await s.Client.PostAsync("/security-fixture/invoices/" + invoice, null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await s.Client.PostAsync("/security-fixture/invoices/" + Guid.NewGuid(), null)).StatusCode);
    }

    [Fact]
    public async Task LoginThrottlingDoesNotBlockWebhookOrSignalRNegotiation()
    {
        await using var s = await Server.Start(1); await s.Google("192.0.2.1");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await s.Google("192.0.2.1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await s.Client.PostAsync("/rate-test/webhook", null)).StatusCode);
        Assert.Null(typeof(PaymentsController).GetMethod("Webhook")!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true).FirstOrDefault());
        s.Client.DefaultRequestHeaders.Authorization = new("Bearer", Server.Token(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.OK, (await s.Client.PostAsync("/hubs/payments/negotiate?negotiateVersion=1", null)).StatusCode);
    }

    [Fact]
    public async Task InternalTrackingErrorIsLoggedButNotReturnedToBrowser()
    {
        await using var s = await Server.Start();
        s.Resident.Setup(x => x.GoogleLoginAsync(It.IsAny<GoogleLoginResidentRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The instance of entity type Account cannot be tracked; private-details"));
        var response = await s.Google("192.0.2.1"); var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("INTERNAL_ERROR", body); Assert.DoesNotContain("private-details", body); Assert.DoesNotContain("cannot be tracked", body);
    }
}

[ApiController]
[Route("security-fixture")]
public sealed class SecurityFixtureController : ControllerBase
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EnableRateLimiting(ApiRateLimits.PaymentLink)]
    [SensitiveOperation("payment-link")]
    [HttpPost("invoices/{id:guid}")]
    public IActionResult Link(Guid id) => Ok(new { id });
}
