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
    public async Task<ActionResult<PagedResponse<OrganizationResidentDto>>> GetResidents(
        Guid organizationId,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var residents = await _organizationResidentService.GetResidentsAsync(
            organizationId,
            status,
            search,
            page,
            pageSize,
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

    [HttpDelete("{organizationId:guid}/residents/invitations/{membershipId:guid}")]
    public async Task<ActionResult> CancelInvitation(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _organizationResidentService.CancelInvitationAsync(
                organizationId,
                membershipId,
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

    [HttpPost("{organizationId:guid}/residents")]
    public async Task<ActionResult<CreateResidentAccountResponse>> CreateAndAddResident(
        Guid organizationId,
        [FromBody] CreateResidentAccountRequest request,
        [FromServices] IResidentService residentService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Create the global resident account
            var response = await residentService.CreateResidentAccountAsync(request, cancellationToken);

            // 2. Add the new resident to the organization immediately as Active
            await _organizationResidentService.AddActiveResidentAsync(
                organizationId,
                response.ResidentId,
                cancellationToken);

            return Created($"/api/resident-memberships", response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete("{organizationId:guid}/residents/{residentId:guid}")]
    public async Task<ActionResult> RemoveResident(
        Guid organizationId,
        Guid residentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _organizationResidentService.RemoveResidentAsync(
                organizationId,
                residentId,
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
}
