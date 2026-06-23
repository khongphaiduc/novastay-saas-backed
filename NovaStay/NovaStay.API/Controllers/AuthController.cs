using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogoutService _logoutService;

    public AuthController(IAuthService authService, ILogoutService logoutService)
    {
        _authService = authService;
        _logoutService = logoutService;
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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await _logoutService.LogoutAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return NoContent();
    }
}
