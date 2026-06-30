using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.API.Controllers;

[Authorize]
[ApiController]
[Route("api/residents")]
public sealed class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;
    private readonly IMinioStorageService _storageService;

    public ResidentsController(IResidentService residentService, IMinioStorageService storageService)
    {
        _residentService = residentService;
        _storageService = storageService;
    }

    [Authorize(Roles = "BusinessOwner")]
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

    [Authorize(Roles = "BusinessOwner")]
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ResidentDto>>> SearchByPhone(
        [FromQuery] string? phone,
        CancellationToken cancellationToken = default)
    {
        var residents = await _residentService.SearchByPhoneAsync(phone, cancellationToken);

        return Ok(residents);
    }

    [Authorize(Roles = "BusinessOwner")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ResidentDto>> UpdateResident(
        Guid id,
        [FromBody] UpdateResidentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resident = await _residentService.UpdateResidentAsync(id, request, cancellationToken);
            return Ok(resident);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "BusinessOwner")]
    [HttpPost("{id}/images/{imageType}")]
    public async Task<ActionResult<ResidentDto>> UploadImage(
        Guid id,
        string imageType,
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _storageService.UploadImageAsync(stream, file.FileName, file.ContentType, cancellationToken);
            var resident = await _residentService.UploadImageAsync(id, imageType, imageUrl, cancellationToken);
            return Ok(resident);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── TASK-053: Hồ sơ cá nhân cư dân ────────────────────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpGet("me")]
    public async Task<ActionResult<ResidentProfileDto>> GetMyProfile(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        try
        {
            var profile = await _residentService.GetMyProfileAsync(accountId, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // ─── TASK-049: Thông tin phòng đang ở + ảnh ─────────────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpGet("me/room")]
    public async Task<ActionResult<ResidentRoomDto>> GetMyRoom(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        var room = await _residentService.GetMyRoomAsync(accountId, cancellationToken);
        if (room == null)
            return NotFound(new { message = "Không tìm thấy phòng đang ở. Hợp đồng có thể chưa được kích hoạt." });

        return Ok(room);
    }

    // ─── TASK-051: Danh sách hợp đồng ───────────────────────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpGet("me/contracts")]
    public async Task<ActionResult<IReadOnlyList<ContractDetailDto>>> GetMyContracts(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        var contracts = await _residentService.GetMyContractsAsync(accountId, cancellationToken);
        return Ok(contracts);
    }

    // ─── TASK-052: Danh sách hóa đơn ────────────────────────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpGet("me/invoices")]
    public async Task<ActionResult<IReadOnlyList<IncomeReceiptDto>>> GetMyInvoices(
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        var invoices = await _residentService.GetMyInvoicesAsync(accountId, cancellationToken);
        return Ok(invoices);
    }

    // ─── TASK-053: Cư dân tự cập nhật hồ sơ cá nhân ──────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpPut("me/profile")]
    public async Task<ActionResult<ResidentProfileDto>> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        try
        {
            var profile = await _residentService.UpdateMyProfileAsync(accountId, request, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // ─── TASK-053: Cư dân tự upload ảnh đại diện ───────────────────────────
    [Authorize(Roles = "Resident")]
    [HttpPost("me/avatar")]
    public async Task<ActionResult<object>> UploadMyAvatar(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required." });

        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return Unauthorized(new { message = "Invalid access token." });

        try
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _residentService.UploadMyAvatarAsync(
                accountId, stream, file.FileName, file.ContentType, cancellationToken);
            return Ok(new { profileImageUrl = imageUrl });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
