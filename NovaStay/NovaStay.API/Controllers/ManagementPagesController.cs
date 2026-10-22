using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.API.Controllers;

[ApiController]
[Authorize(Roles = "BusinessOwner")]
[Route("api")]
public sealed class ManagementPagesController(ManagementPagingService service) : ControllerBase
{
    private Guid AccountId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException();

    [HttpGet("organizations/{organizationId:guid}/properties/{propertyId:guid}/income-receipts/paged")]
    public Task<IActionResult> Income(Guid organizationId, Guid propertyId, int pageIndex = 1, int pageSize = 10,
        ApprovalStatus? status = null, PaymentMethod? paymentMethod = null, string? search = null, CancellationToken ct = default) =>
        Run(async () => await service.Income(AccountId, organizationId, propertyId, pageIndex, pageSize, status, paymentMethod, search, ct));

    [HttpGet("organizations/{organizationId:guid}/properties/{propertyId:guid}/expenses/paged")]
    public Task<IActionResult> Expenses(Guid organizationId, Guid propertyId, int pageIndex = 1, int pageSize = 10,
        ApprovalStatus? status = null, PaymentMethod? paymentMethod = null, string? search = null, CancellationToken ct = default) =>
        Run(async () => await service.Expenses(AccountId, organizationId, propertyId, pageIndex, pageSize, status, paymentMethod, search, ct));

    [HttpGet("utility-meters/by-property/{propertyId:guid}/paged")]
    public Task<IActionResult> PropertyMeters(Guid propertyId, int pageIndex = 1, int pageSize = 10, string? search = null, CancellationToken ct = default) =>
        Run(async () => await service.Meters(AccountId, propertyId, null, pageIndex, pageSize, search, ct));

    [HttpGet("utility-meters/by-room/{roomId:guid}/paged")]
    public Task<IActionResult> RoomMeters(Guid roomId, int pageIndex = 1, int pageSize = 10, string? search = null, CancellationToken ct = default) =>
        Run(async () => await service.Meters(AccountId, null, roomId, pageIndex, pageSize, search, ct));

    [HttpGet("utility-meters/{meterId:guid}/readings/paged")]
    public Task<IActionResult> Readings(Guid meterId, int pageIndex = 1, int pageSize = 10, string? billingPeriod = null, CancellationToken ct = default) =>
        Run(async () => await service.Readings(AccountId, meterId, pageIndex, pageSize, billingPeriod, ct));

    private async Task<IActionResult> Run(Func<Task<object>> action)
    {
        try { return Ok(await action()); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
    }
}
