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

    [HttpPost]
    public async Task<ActionResult<CreateResidentAccountResponse>> CreateResidentAccount(
        [FromBody] CreateResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _residentService.CreateResidentAccountAsync(
                request,
                cancellationToken);

            return Created($"/api/residents/{response.ResidentId}", response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
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
