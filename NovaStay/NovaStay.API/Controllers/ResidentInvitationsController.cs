using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "Resident")]
[ApiController]
[Route("api/resident-invitations")]
public sealed class ResidentInvitationsController : ControllerBase
{
    private readonly IResidentInvitationService _residentInvitationService;

    public ResidentInvitationsController(IResidentInvitationService residentInvitationService)
    {
        _residentInvitationService = residentInvitationService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<ResidentInvitationDto>>> GetPendingInvitations(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(new { message = "Invalid access token." });
        }

        var invitations = await _residentInvitationService.GetPendingInvitationsAsync(
            accountId,
            cancellationToken);

        return Ok(invitations);
    }
}
