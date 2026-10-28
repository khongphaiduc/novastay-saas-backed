using Microsoft.AspNetCore.RateLimiting;
using NovaStay.API.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

/// <summary>
/// Quản lý Invitation Links — Chủ trọ tạo link, người thuê dùng link để gia nhập
/// </summary>
[ApiController]
public sealed class InvitationLinksController : ControllerBase
{
    private readonly IInvitationLinkService _invitationLinkService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public InvitationLinksController(
        IInvitationLinkService invitationLinkService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _invitationLinkService = invitationLinkService;
        _configuration = configuration;
        _environment = environment;
    }

    private string GetFrontendBaseUrl()
    {
        // 1. Cấu hình FrontendUrl (từ appsettings.json hoặc .env)
        var configUrl = _configuration["FrontendUrl"];
        if (!string.IsNullOrWhiteSpace(configUrl))
        {
            return configUrl.TrimEnd('/');
        }

        // 2. HTTP Origin header (browser gửi qua fetch/CORS)
        var origin = Request.Headers["Origin"].ToString();
        if (!string.IsNullOrWhiteSpace(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
        {
            return $"{originUri.Scheme}://{originUri.Authority}";
        }

        // 3. HTTP Referer header
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            return $"{refererUri.Scheme}://{refererUri.Authority}";
        }

        // 4. Mặc định theo môi trường
        return _environment.IsDevelopment() ? "http://localhost:5173" : "https://nestone.io.vn";
    }

    // ─── Chủ trọ: Tạo link mời ──────────────────────────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [EnableRateLimiting(ApiRateLimits.Notice)]
    [HttpPost("api/organizations/{organizationId:guid}/invitation-links")]
    public async Task<ActionResult<InvitationLinkDto>> CreateLink(
        Guid organizationId,
        [FromBody] CreateInvitationLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = GetFrontendBaseUrl();
        try
        {
            var result = await _invitationLinkService.CreateAsync(organizationId, request, baseUrl, cancellationToken);
            return Created($"/invitation/{result.Token}", result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống khi tạo link mời.", detail = ex.Message });
        }
    }

    // ─── Chủ trọ: Xem danh sách link ────────────────────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [HttpGet("api/organizations/{organizationId:guid}/invitation-links")]
    public async Task<ActionResult<IReadOnlyList<InvitationLinkDto>>> GetLinks(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = GetFrontendBaseUrl();
        var result = await _invitationLinkService.GetByOrganizationAsync(organizationId, baseUrl, cancellationToken);
        return Ok(result);
    }

    // ─── Chủ trọ: Vô hiệu hoá link ──────────────────────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [HttpDelete("api/organizations/{organizationId:guid}/invitation-links/{linkId:guid}")]
    public async Task<ActionResult> DeactivateLink(
        Guid organizationId,
        Guid linkId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _invitationLinkService.DeactivateAsync(organizationId, linkId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Người thuê: Xem preview link trước khi accept (không cần đăng nhập) ─
    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Invitation)]
    [HttpGet("api/invitation/{token}")]
    public async Task<ActionResult<InvitationLinkPreviewDto>> Preview(
        string token,
        CancellationToken cancellationToken = default)
    {
        var preview = await _invitationLinkService.GetPreviewAsync(token, cancellationToken);
        return Ok(preview);
    }

    // ─── Người thuê: Chấp nhận link (phải đăng nhập) ────────────────────────
    [Authorize(Roles = "Resident")]
    [EnableRateLimiting(ApiRateLimits.Invitation)]
    [HttpPost("api/invitation/{token}/accept")]
    public async Task<ActionResult<ResidentMembershipResponse>> Accept(
        string token,
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        try
        {
            var result = await _invitationLinkService.AcceptAsync(accountId, token, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ─── Chủ trọ: Danh sách người thuê chờ duyệt ──────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [HttpGet("api/organizations/{organizationId:guid}/invitation-links/pending-approvals")]
    public async Task<ActionResult<IReadOnlyList<PendingResidentApprovalDto>>> GetPendingApprovals(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _invitationLinkService.GetPendingApprovalsAsync(organizationId, cancellationToken);
        return Ok(result);
    }

    // ─── Chủ trọ: Duyệt người thuê ──────────────────────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [HttpPost("api/organizations/{organizationId:guid}/invitation-links/memberships/{membershipId:guid}/approve")]
    public async Task<ActionResult<ResidentMembershipResponse>> ApproveResident(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invitationLinkService.ApprovePendingResidentAsync(organizationId, membershipId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Chủ trọ: Từ chối người thuê ────────────────────────────────────────
    [Authorize(Roles = "BusinessOwner")]
    [HttpPost("api/organizations/{organizationId:guid}/invitation-links/memberships/{membershipId:guid}/reject")]
    public async Task<ActionResult<ResidentMembershipResponse>> RejectResident(
        Guid organizationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invitationLinkService.RejectPendingResidentAsync(organizationId, membershipId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
