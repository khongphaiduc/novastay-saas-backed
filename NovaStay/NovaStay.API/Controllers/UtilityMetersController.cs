using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

/// <summary>
/// Quản lý đồng hồ dịch vụ (điện, nước...) và ghi nhận chỉ số hàng tháng
/// </summary>
[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/utility-meters")]
public sealed class UtilityMetersController : ControllerBase
{
    private readonly IUtilityService _utilityService;

    public UtilityMetersController(IUtilityService utilityService)
    {
        _utilityService = utilityService;
    }

    /// <summary>Lấy danh sách đồng hồ của một phòng (kèm chỉ số mới nhất)</summary>
    [HttpGet("by-room/{roomId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UtilityMeterDto>>> GetByRoom(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var meters = await _utilityService.GetMetersByRoomAsync(roomId, cancellationToken);
        return Ok(meters);
    }

    /// <summary>Lấy danh sách đồng hồ theo property (để ghi hàng loạt)</summary>
    [HttpGet("by-property/{propertyId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UtilityMeterDto>>> GetByProperty(
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var meters = await _utilityService.GetMetersByPropertyAsync(propertyId, cancellationToken);
        return Ok(meters);
    }

    /// <summary>Tạo mới đồng hồ dịch vụ cho phòng</summary>
    [HttpPost]
    public async Task<ActionResult<UtilityMeterDto>> CreateMeter(
        [FromBody] CreateUtilityMeterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var meter = await _utilityService.CreateMeterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByRoom), new { roomId = meter.RoomId }, meter);
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

    /// <summary>Ghi chỉ số mới cho một đồng hồ</summary>
    [HttpPost("{meterId:guid}/readings")]
    public async Task<ActionResult<UtilityReadingDto>> RecordReading(
        Guid meterId,
        [FromBody] RecordUtilityReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reading = await _utilityService.RecordReadingAsync(meterId, request, cancellationToken);
            return Ok(reading);
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

    /// <summary>Lấy lịch sử ghi số của một đồng hồ</summary>
    [HttpGet("{meterId:guid}/readings")]
    public async Task<ActionResult<IReadOnlyList<UtilityReadingDto>>> GetReadings(
        Guid meterId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var readings = await _utilityService.GetReadingsByMeterAsync(meterId, cancellationToken);
            return Ok(readings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
