using NovaStay.Application.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NovaStay.API.Security;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.API.Controllers;

[ApiController, Authorize(Roles = "BusinessOwner")]
public sealed class RoomOccupanciesController(RoomOccupancyService service) : ControllerBase
{
    private Guid Owner => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    private async Task<IActionResult> Run<T>(Func<Task<T>> action)
    {
        if (Owner == Guid.Empty) return Unauthorized();
        try { return Ok(await action()); }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (OverflowException) { return BadRequest(new { message = "Phân trang không hợp lệ." }); }
        catch (RoomOccupancyException e)
        { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "Dữ liệu vừa thay đổi. Vui lòng tải lại và thử lại." }); }
    }

    [HttpGet("api/organizations/{organizationId:guid}/room-occupancies")]
    public Task<IActionResult> List(Guid organizationId, CancellationToken ct) => Run(() => service.List(Owner, organizationId, ct));

    [HttpGet("api/contracts/{id:guid}/occupants")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => Run(() => service.Get(Owner, id, ct));

    [HttpGet("api/contracts/{id:guid}/co-resident-candidates")]
    public Task<IActionResult> Candidates(Guid id, [FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default) => Run(() => service.Candidates(Owner, id, search, page, pageSize, ct));

    [HttpPut("api/contracts/{id:guid}/co-residents"), EnableRateLimiting(ApiRateLimits.RoomOccupancy), RequestSizeLimit(4096)]
    public Task<IActionResult> Set(Guid id, SetCoResidentsRequest request, CancellationToken ct) =>
        Run(() => service.Set(Owner, id, request.ResidentIds, ct));
}
