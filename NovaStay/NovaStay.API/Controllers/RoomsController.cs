using NovaStay.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.API.Extensions;

namespace NovaStay.API.Controllers;
[Authorize(Roles = "BusinessOwner")]
[ApiController]
[OwnerData]
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
        try
        {
            var room = await _roomService.CreateRoomAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRooms), new { propertyId = room.PropertyId }, room);
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
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
        catch (NovaStay.Application.Common.RoomOccupancyException ex) { return Conflict(new { message = ex.Message }); }
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
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting(NovaStay.API.Security.ApiRateLimits.ProxyImage)]
    public async Task<IActionResult> ProxyImage(
        [FromQuery] string url,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken = default,
        [FromServices] IHttpClientFactory? httpClientFactory = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new { message = "URL is required" });
        }

        var root = configuration["Storage:PublicBaseUrl"] ?? configuration["MinIO:PublicEndpoint"] ?? "https://nestone.io.vn/uploads";
        if (!Uri.TryCreate(root, UriKind.Absolute, out var storage) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var target) ||
            target.Scheme != storage.Scheme || target.Host != storage.Host || target.Port != storage.Port ||
            !string.IsNullOrEmpty(target.UserInfo) ||
            !target.AbsolutePath.StartsWith(storage.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal))
            return BadRequest(new { message = "Chỉ hỗ trợ ảnh từ kho lưu trữ đã cấu hình." });
        try
        {
            var httpClient = httpClientFactory?.CreateClient("storage-proxy")
                ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(15), MaxResponseContentBufferSize = 10 * 1024 * 1024 };
            using var response = await httpClient.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return NotFound();
            var type = response.Content.Headers.ContentType?.MediaType;
            if (type is not ("image/png" or "image/jpeg" or "image/webp" or "image/gif")) return BadRequest(new { message = "Định dạng ảnh không được hỗ trợ." });
            if (response.Content.Headers.ContentLength > 10 * 1024 * 1024) return StatusCode(413, new { message = "Ảnh quá lớn." });
            Response.Headers.CacheControl = "public, max-age=3600";
            return File(await response.Content.ReadAsByteArrayAsync(cancellationToken), type);
        }
        catch (HttpRequestException) { return StatusCode(502, new { message = "Không tải được ảnh từ kho lưu trữ." }); }
    }
}
