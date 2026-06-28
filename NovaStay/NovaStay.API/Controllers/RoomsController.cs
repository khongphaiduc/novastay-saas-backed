using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.API.Extensions;

namespace NovaStay.API.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// TASK-011: Lấy danh sách phòng
    /// TASK-012: Tìm kiếm phòng theo tên/mã phòng (query param: search)
    /// TASK-013: Lọc phòng theo trạng thái (query param: status)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomDto>>> GetRooms(
        [FromQuery] Guid propertyId,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rooms = await _roomService.GetRoomsAsync(propertyId, search, status, pageIndex, pageSize, cancellationToken);
            return Ok(rooms);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-014: Thêm phòng mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _roomService.CreateRoomAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRooms), new { propertyId = room.PropertyId }, room);
    }

    /// <summary>
    /// TASK-015: Cập nhật thông tin phòng
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoomDto>> UpdateRoom(
        Guid id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await _roomService.UpdateRoomAsync(id, request, cancellationToken);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-016: Xóa phòng
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoom(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _roomService.DeleteRoomAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-017: Upload ảnh phòng (multipart/form-data)
    /// </summary>
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<RoomImageDto>> UploadImage(
        Guid id,
        IFormFile? image,
        [FromForm] bool isCover = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = image.ToUploadRequest(isCover);
            var imageDto = await _roomService.UploadRoomImageAsync(id, request, cancellationToken);
            return Ok(imageDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-018: Cập nhật giá thuê phòng
    /// </summary>
    [HttpPatch("{id:guid}/price")]
    public async Task<ActionResult<RoomDto>> UpdatePrice(
        Guid id,
        [FromBody] UpdateBasePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await _roomService.UpdateBasePriceAsync(id, request, cancellationToken);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-019: Cập nhật sức chứa phòng
    /// </summary>
    [HttpPatch("{id:guid}/occupants")]
    public async Task<ActionResult<RoomDto>> UpdateOccupants(
        Guid id,
        [FromBody] UpdateMaxOccupantsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await _roomService.UpdateMaxOccupantsAsync(id, request, cancellationToken);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// TASK-020: Cập nhật tiện ích phòng
    /// </summary>
    [HttpPatch("{id:guid}/amenities")]
    public async Task<ActionResult<RoomDto>> UpdateAmenities(
        Guid id,
        [FromBody] UpdateAmenitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await _roomService.UpdateAmenitiesAsync(id, request, cancellationToken);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Xóa ảnh phòng
    /// </summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid id,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _roomService.DeleteRoomImageAsync(id, imageId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Proxy ảnh từ MinIO để vượt rào Mixed Content / SSL Error
    /// </summary>
    [HttpGet("proxy-image")]
    [AllowAnonymous]
    public async Task<IActionResult> ProxyImage(
        [FromQuery] string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new { message = "URL is required" });
        }

        try
        {
            using var httpClient = new HttpClient();
            // Lấy trực tiếp bằng giao thức HTTP nội bộ của server (hoặc URL mà Sếp config)
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Failed to fetch image" });
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Proxy error: " + ex.Message });
        }
    }
}
