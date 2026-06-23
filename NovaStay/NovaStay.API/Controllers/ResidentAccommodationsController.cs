using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "Resident")]
[ApiController]
[Route("api/residents/me/accommodations")]
public sealed class ResidentAccommodationsController : ControllerBase
{
    private readonly IResidentService _residentService;

    public ResidentAccommodationsController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResidentAccommodationDto>>> GetActiveAccommodations(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new { message = "Invalid access token." });
        }

        var accommodations = await _residentService.GetActiveAccommodationsAsync(
            accountId,
            cancellationToken);

        return Ok(accommodations);
    }

}
