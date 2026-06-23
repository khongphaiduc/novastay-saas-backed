using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System.Security.Claims;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "Resident")]
[ApiController]
[Route("api/resident-memberships")]
public sealed class ResidentMembershipsController : ControllerBase
{
    private readonly IOrganizationResidentService _organizationResidentService;

    public ResidentMembershipsController(IOrganizationResidentService organizationResidentService)
    {
        _organizationResidentService = organizationResidentService;
    }

    [HttpPost("{membershipId:guid}/accept")]
    public async Task<ActionResult<ResidentMembershipResponse>> AcceptInvitation(
        Guid membershipId,
        [FromBody] ResidentInvitationDecisionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new { message = "Invalid access token." });
        }

        try
        {
            var response = await _organizationResidentService.AcceptInvitationAsync(
                accountId,
                membershipId,
                request?.IsAccepted ?? true,
                cancellationToken);

            return Ok(response);
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
