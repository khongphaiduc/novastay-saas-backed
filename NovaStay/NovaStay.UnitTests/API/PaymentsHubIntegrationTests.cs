using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NovaStay.API.Hubs;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.UnitTests.API;

public sealed class PaymentsHubIntegrationTests
{
    [Fact]
    public async Task Hub_AuthenticatesJwt_AndNotifiesOnlyInvoiceOwnerAndResident()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('t', 64)));
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ContentRootPath = AppContext.BaseDirectory });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IUserIdProvider, PaymentUserIdProvider>();
        builder.Services.AddScoped<IPaymentNotifier, SignalRPaymentNotifier>();
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidIssuer = "test", ValidateAudience = true, ValidAudience = "test",
                ValidateIssuerSigningKey = true, IssuerSigningKey = key, ValidateLifetime = true
            };
            options.Events = new() { OnMessageReceived = PaymentHubAuthentication.ReadToken };
        });
        await using var app = builder.Build();
        app.UseAuthentication(); app.UseAuthorization();
        app.MapHub<PaymentsHub>("/hubs/payments");
        app.MapGet("/private", () => "private").RequireAuthorization();
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient();
            using var anonymous = await client.PostAsync(address + "/hubs/payments/negotiate?negotiateVersion=1", new StringContent(""));
            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            var owner = Guid.NewGuid(); var resident = Guid.NewGuid();
            string Token(Guid id, string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                "test", "test", [new Claim(ClaimTypes.NameIdentifier, id.ToString().ToUpperInvariant()), new Claim(ClaimTypes.Role, role)],
                expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));
            using var ordinaryApi = await client.GetAsync(address + "/private?access_token=" + Token(owner, "BusinessOwner"));
            Assert.Equal(HttpStatusCode.Unauthorized, ordinaryApi.StatusCode); // Query JWT works only for the hub.
            client.DefaultRequestHeaders.Authorization = new("Bearer", Token(Guid.NewGuid(), "Admin"));
            using var forbidden = await client.PostAsync(address + "/hubs/payments/negotiate?negotiateVersion=1", new StringContent(""));
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            using var ownerSocket = await Connect(address, Token(owner, "BusinessOwner"));
            using var residentSocket = await Connect(address, Token(resident, "Resident"));
            using var strangerSocket = await Connect(address, Token(Guid.NewGuid(), "Resident"));
            using var scope = app.Services.CreateScope();
            var message = new InvoicePaymentUpdated(Guid.NewGuid(), Guid.NewGuid(), "Paid", "PayOS", DateTime.UtcNow);
            await scope.ServiceProvider.GetRequiredService<IPaymentNotifier>().NotifyAsync(owner, resident, message, default);
            Assert.Contains(message.EventId.ToString(), await Receive(ownerSocket, TimeSpan.FromSeconds(3)));
            Assert.Contains(message.EventId.ToString(), await Receive(residentSocket, TimeSpan.FromSeconds(3)));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Receive(strangerSocket, TimeSpan.FromMilliseconds(250)));
        }
        finally { await app.StopAsync(); }
    }

    private static async Task<ClientWebSocket> Connect(string address, string token)
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri(address.Replace("http://", "ws://") + "/hubs/payments?access_token=" + Uri.EscapeDataString(token)), default);
        await socket.SendAsync(Encoding.UTF8.GetBytes("{\"protocol\":\"json\",\"version\":1}\u001e"), WebSocketMessageType.Text, true, default);
        Assert.Contains("{}", await Receive(socket, TimeSpan.FromSeconds(3)));
        return socket;
    }

    private static async Task<string> Receive(ClientWebSocket socket, TimeSpan timeout)
    {
        using var cancel = new CancellationTokenSource(timeout);
        var buffer = new byte[8192];
        using var text = new MemoryStream();
        do
        {
            var result = await socket.ReceiveAsync(buffer.AsMemory(), cancel.Token);
            text.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) break;
        } while (true);
        return Encoding.UTF8.GetString(text.ToArray());
    }
}
