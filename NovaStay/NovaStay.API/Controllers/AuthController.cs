using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using NovaStay.API.Security;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System.Security.Claims;

namespace NovaStay.API.Controllers;

[ApiController]
[RequestSizeLimit(16 * 1024)]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IChangePasswordService _changePasswordService;
    private readonly ILogoutService _logoutService;
    private readonly IResidentAuthService _residentAuthService;
    private readonly IProvideAccessToken _provideNewAccessToken;

    public AuthController(
        IAuthService authService,
        IChangePasswordService changePasswordService,
        ILogoutService logoutService,
        IResidentAuthService residentAuthService,
        IProvideAccessToken provideAccessToken)
    {
        _authService = authService;
        _changePasswordService = changePasswordService;
        _logoutService = logoutService;
        _residentAuthService = residentAuthService;
        _provideNewAccessToken = provideAccessToken;
    }

    [EnableRateLimiting(ApiRateLimits.Register)]
    [HttpPost("register-organization")]
    public async Task<ActionResult<RegisterOrganizationAccountResponse>> RegisterOrganization([FromBody] RegisterOrganizationAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authService.RegisterOrganizationOwnerAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Created($"/api/organizations/{response.OrganizationId}", response);
        }
        catch (InvalidOperationException exception) when (SafeAuthErrors.IsExpected(exception))
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [EnableRateLimiting(ApiRateLimits.Login)]
    [SensitiveOperation("login")]
    [HttpPost("business/login")]
    public async Task<ActionResult<LoginBusinessAccountResponse>> LoginBusiness([FromBody] LoginBusinessAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authService.LoginBusinessOwnerAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Ok(response);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    // tetst
    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Reset)]
    [SensitiveOperation("reset")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _authService.ResetPasswordAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Accepted(new { message = "Nếu tài khoản phù hợp tồn tại, hướng dẫn sẽ được gửi tới email của bạn." });
        }
        catch (KeyNotFoundException)
        {
            return Accepted(new { message = "Nếu tài khoản phù hợp tồn tại, hướng dẫn sẽ được gửi tới email của bạn." });
        }
        catch (InvalidOperationException exception) when (exception.Message == "Account is inactive.")
        {
            return Accepted(new { message = "Nếu tài khoản phù hợp tồn tại, hướng dẫn sẽ được gửi tới email của bạn." });
        }
        catch (InvalidOperationException exception) when (exception.Message == "Email is required.")
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [EnableRateLimiting(ApiRateLimits.Login)]
    [SensitiveOperation("login")]
    [HttpPost("resident/login")]
    public async Task<ActionResult<LoginResidentAccountResponse>> LoginResident(
        [FromBody] LoginResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _residentAuthService.LoginAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Ok(response);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Register)]
    [HttpPost("resident/register")]
    public async Task<ActionResult<RegisterResidentAccountResponse>> RegisterResident(
        [FromBody] RegisterResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _residentAuthService.RegisterAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Created($"/api/residents/{response.ResidentId}", response);
        }
        catch (InvalidOperationException exception) when (SafeAuthErrors.IsExpected(exception))
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Login)]
    [HttpPost("resident/google-login")]
    public async Task<ActionResult<LoginResidentAccountResponse>> GoogleLoginResident(
        [FromBody] GoogleLoginResidentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _residentAuthService.GoogleLoginAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception) when (SafeAuthErrors.IsExpected(exception))
        {
            return Conflict(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await _logoutService.LogoutAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [EnableRateLimiting(ApiRateLimits.ChangePassword)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new { message = "Invalid access token." });
        }

        try
        {
            await _changePasswordService.ChangePasswordAsync(
                accountId,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return NoContent();
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (InvalidOperationException exception) when (SafeAuthErrors.IsExpected(exception))
        {
            return BadRequest(new { message = exception.Message });
        }
    }


    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Refresh)]
    [SensitiveOperation("refresh")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _provideNewAccessToken.GetAccessToken(request.RefreshToken);

        return Ok(result);
    }


}
