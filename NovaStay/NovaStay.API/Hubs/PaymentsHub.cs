using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace NovaStay.API.Hubs;

// Recipients come exclusively from invoice ownership; clients cannot join arbitrary groups.
[Authorize(Roles = "BusinessOwner,Resident")]
public sealed class PaymentsHub : Hub;

public static class PaymentHubAuthentication
{
    public static Task ReadToken(MessageReceivedContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/hubs/payments") &&
            context.Request.Query.TryGetValue("access_token", out var token))
            context.Token = token.ToString();
        return Task.CompletedTask;
    }
}

public sealed class PaymentUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        Guid.TryParse(connection.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id.ToString() : null;
}

public sealed class SignalRPaymentNotifier(IHubContext<PaymentsHub> hub) : IPaymentNotifier
{
    public Task NotifyAsync(Guid ownerAccountId, Guid? residentAccountId, InvoicePaymentUpdated message, CancellationToken ct)
    {
        var users = new[] { ownerAccountId, residentAccountId ?? ownerAccountId }.Distinct().Select(x => x.ToString()).ToArray();
        return hub.Clients.Users(users).SendAsync("InvoicePaymentUpdated", message, ct);
    }
}
