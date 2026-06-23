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

    [HttpGet("{organizationId:guid}/residents/invitations")]
    public async Task<ActionResult<IReadOnlyList<OrganizationResidentInvitationDto>>> GetResidentInvitations(
        Guid organizationId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var invitations = await _organizationResidentService.GetResidentInvitationsAsync(
            organizationId,
            status,
            cancellationToken);

        if (invitations is null)
        {
            return NotFound(new { message = "Organization not found." });
        }

        return Ok(invitations);
    }

    [HttpPost("{organizationId:guid}/residents/invitations")]
    public async Task<ActionResult<ResidentMembershipResponse>> InviteResident(
        Guid organizationId,
        [FromBody] InviteResidentToOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _organizationResidentService.InviteResidentAsync(
                organizationId,
                request,
                cancellationToken);

            return Created($"/api/resident-memberships/{response.MembershipId}", response);
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
}
