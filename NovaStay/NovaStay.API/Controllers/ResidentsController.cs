using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/residents")]
public sealed class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;

    public ResidentsController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ResidentDto>>> SearchByPhone(
        [FromQuery] string? phone,
        CancellationToken cancellationToken = default)
    {
        var residents = await _residentService.SearchByPhoneAsync(phone, cancellationToken);

        return Ok(residents);
    }
}
