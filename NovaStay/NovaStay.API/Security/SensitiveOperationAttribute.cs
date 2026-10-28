using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NovaStay.Application.DTOs;

namespace NovaStay.API.Security;

public sealed class SensitiveOperationAttribute : TypeFilterAttribute
{
    public SensitiveOperationAttribute(string operation) : base(typeof(SensitiveOperationFilter)) { Arguments = [operation]; }
}

public sealed class SensitiveOperationFilter(SensitiveRequestGate gate, string operation) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var user = ApiRateLimits.UserOrIp(http);
        var target = operation switch
        {
            "login" => context.ActionArguments.Values.OfType<LoginResidentAccountRequest>().FirstOrDefault()?.Sdt
                ?? context.ActionArguments.Values.OfType<LoginBusinessAccountRequest>().FirstOrDefault()?.Email,
            "reset" => context.ActionArguments.Values.OfType<ResetPasswordRequest>().FirstOrDefault()?.Email,
            "refresh" => context.ActionArguments.Values.OfType<RefreshTokenRequest>().FirstOrDefault()?.RefreshToken,
            _ => context.ActionArguments.TryGetValue("id", out var id) ? id?.ToString()
                : context.ActionArguments.TryGetValue("membershipId", out var membership) ? membership?.ToString() : null
        };
        if (operation is "login" or "reset") target = target?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(target)) { await next(); return; }
        var key = operation is "login" ? gate.Key(operation, ApiRateLimits.ClientIp(http), target)
            : operation is "reset" or "refresh" ? gate.Key(operation, target) : gate.Key(operation, user, target);
        var retry = operation switch
        {
            "login" => gate.BeginLogin(key),
            "reset" => gate.Reserve(key, 5, TimeSpan.FromHours(1), TimeSpan.FromMinutes(2)),
            "refresh" => gate.Reserve(key, 30, TimeSpan.FromMinutes(1)),
            "payment-link" => gate.Reserve(key, 20, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(3)),
            "reconcile" => gate.Reserve(key, 20, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5)),
            "notice" => gate.Reserve(key, 10, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(1)),
            _ => 0
        };
        if (retry > 0)
        {
            http.Response.Headers.RetryAfter = retry.ToString(CultureInfo.InvariantCulture);
            context.Result = new ObjectResult(ApiRateLimits.Error(retry)) { StatusCode = 429 }; return;
        }
        if (operation != "login") { await next(); return; }
        var failed = false; var succeeded = false;
        try
        {
            var executed = await next();
            var status = (executed.Result as ObjectResult)?.StatusCode ?? (executed.Result as StatusCodeResult)?.StatusCode;
            failed = status == 401;
            succeeded = executed.Exception == null && status is >= 200 and < 300;
        }
        finally { gate.EndLogin(key, failed, succeeded); }
    }
}
