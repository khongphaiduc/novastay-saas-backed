using NovaStay.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

/// <summary>
/// Maintenance Management: TASK-021 và TASK-022
/// </summary>
[Authorize(Roles = "BusinessOwner")]
[ApiController]
[OwnerData]
[Route("api/maintenance")]
public sealed class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>TASK-021: Xem lịch sử bảo trì của phòng</summary>
    [HttpGet("rooms/{roomId:guid}")]
    public async Task<ActionResult<PagedResult<MaintenanceTicketDetailDto>>> GetByRoom(
        Guid roomId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pagedResult = await _maintenanceService.GetByRoomAsync(roomId, pageIndex, pageSize, cancellationToken);
            return Ok(pagedResult);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>TASK-022: Đánh dấu phòng đang bảo trì / trống</summary>
    [HttpPatch("rooms/{roomId:guid}/status")]
    public async Task<IActionResult> MarkRoomStatus(
        Guid roomId,
        [FromBody] MarkRoomStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var finalStatus = await _maintenanceService.MarkRoomStatusAsync(
                roomId,
                request.Status,
                request.OrganizationId,
                request.ResidentId,
                request.Description,
                cancellationToken);
            return Ok(new { status = finalStatus });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Chủ nhà xem phiếu sự cố / bảo trì của cơ sở (lọc theo trạng thái)</summary>
    [HttpGet("tickets")]
    public async Task<ActionResult<IReadOnlyList<MaintenanceTicketDetailDto>>> GetTickets(
        [FromQuery] Guid organizationId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _maintenanceService.GetOrganizationTicketsAsync(organizationId, status, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Chủ nhà cập nhật trạng thái phiếu</summary>
    [HttpPatch("tickets/{id:guid}/status")]
    public async Task<ActionResult<MaintenanceTicketDetailDto>> UpdateTicketStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _maintenanceService.UpdateTicketStatusAsync(id, request.Status, cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>TASK-021: Tạo phiếu bảo trì thủ công</summary>
    [HttpPost("tickets")]
    public async Task<ActionResult<MaintenanceTicketDetailDto>> CreateTicket(
        [FromBody] CreateMaintenanceTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticket = await _maintenanceService.CreateTicketAsync(request, cancellationToken);
            return Created($"/api/maintenance/tickets/{ticket.Id}", ticket);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public sealed class UpdateTicketStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class MarkRoomStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public Guid? ResidentId { get; set; }
    public string? Description { get; set; }
}
