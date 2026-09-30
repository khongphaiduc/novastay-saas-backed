using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System.Security.Claims;

namespace NovaStay.API.Controllers;

[ApiController]
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
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

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
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _authService.ResetPasswordAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await _logoutService.LogoutAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return NoContent();
    }

    [Authorize]
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
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }


    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _provideNewAccessToken.GetAccessToken(request.RefreshToken);

        return Ok(result);
    }


}
