using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}
