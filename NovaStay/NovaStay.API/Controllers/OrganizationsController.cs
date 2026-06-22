using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;
[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/organizations")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IOrganizationResidentService _organizationResidentService;

    public OrganizationsController(IOrganizationResidentService organizationResidentService)
    {
        _organizationResidentService = organizationResidentService;
    }

    [HttpGet("{organizationId:guid}/residents")]
    public async Task<ActionResult<IReadOnlyList<OrganizationResidentDto>>> GetResidents(
        Guid organizationId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var residents = await _organizationResidentService.GetResidentsAsync(
            organizationId,
            status,
            cancellationToken);

        if (residents is null)
        {
            return NotFound(new { message = "Organization not found." });
        }

        return Ok(residents);
    }
}
