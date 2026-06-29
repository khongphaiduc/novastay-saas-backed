using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/assets")]
public sealed class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    /// <summary>
    /// TASK-055: Lấy danh sách tài sản của Organization
    /// Query: ?organizationId=...&search=...&category=...&status=...
    /// Status hợp lệ: Good, Damaged, Maintenance, Broken, Working
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAssets(
        [FromQuery] Guid organizationId,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assets = await _assetService.GetAssetsAsync(
                organizationId, search, category, status, cancellationToken);
            return Ok(assets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Task phát sinh: Lấy danh sách tài sản đang trong 1 phòng
    /// GET /api/assets/by-room?roomId=...
    /// </summary>
    [HttpGet("by-room")]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAssetsByRoom(
        [FromQuery] Guid roomId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assets = await _assetService.GetAssetsByRoomAsync(roomId, cancellationToken);
            return Ok(assets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Task phát sinh: Thống kê tài sản của Organization
    /// GET /api/assets/statistics?organizationId=...
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AssetStatisticsDto>> GetStatistics(
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _assetService.GetStatisticsAsync(organizationId, cancellationToken);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-056: Thêm tài sản mới
    /// POST /api/assets
    /// Body: { organizationId, assetName, category, brand, model, assetCode, purchaseDate, warrantyExpiryDate, baseValue, initialNote }
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AssetDto>> CreateAsset(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _assetService.CreateAssetAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAssets), new { organizationId = asset.OrganizationId }, asset);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-057: Cập nhật thông tin tài sản
    /// PUT /api/assets/{id}
    /// Body: { assetName, category, brand, model, assetCode, purchaseDate, warrantyExpiryDate, baseValue }
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetDto>> UpdateAsset(
        Guid id,
        [FromBody] UpdateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _assetService.UpdateAssetAsync(id, request, cancellationToken);
            return Ok(asset);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-056: Xóa mềm tài sản
    /// DELETE /api/assets/{id}
    /// Lưu ý: Không thể xóa tài sản đang gán vào phòng
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsset(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _assetService.DeleteAssetAsync(id, cancellationToken);
            return NoContent();
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

    /// <summary>
    /// TASK-058: Gán tài sản vào phòng
    /// POST /api/assets/{id}/assign
    /// Body: { roomId, note }
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<AssetHistoryDto>> AssignAsset(
        Guid id,
        [FromBody] AssignAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _assetService.AssignAssetToRoomAsync(id, request, cancellationToken);
            return Ok(history);
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

    /// <summary>
    /// TASK-059: Thu hồi tài sản khỏi phòng (đưa về kho)
    /// POST /api/assets/{id}/revoke
    /// Body: { note }
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    public async Task<ActionResult<AssetHistoryDto>> RevokeAsset(
        Guid id,
        [FromBody] RevokeAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _assetService.RevokeAssetFromRoomAsync(id, request, cancellationToken);
            return Ok(history);
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

    /// <summary>
    /// TASK-060 / TASK-061: Cập nhật tình trạng / Ghi nhận hư hỏng
    /// PATCH /api/assets/{id}/status
    /// Body: { status, note }
    /// Status hợp lệ: Good, Working, Damaged, Maintenance, Broken
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AssetHistoryDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateAssetStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _assetService.UpdateAssetStatusAsync(id, request, cancellationToken);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-062: Xem lịch sử luân chuyển của một tài sản
    /// GET /api/assets/{id}/history
    /// </summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<AssetHistoryDto>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _assetService.GetAssetHistoryAsync(id, cancellationToken);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
