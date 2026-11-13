using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.API.Controllers;

[ApiController, Authorize(Roles = "BusinessOwner")]
public sealed class OwnerStatisticsController(HostContext db) : ControllerBase
{
    private Guid Owner => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    [HttpGet("api/rooms/summary")]
    public async Task<IActionResult> Rooms(Guid propertyId, CancellationToken ct)
    {
        if (!await db.Properties.AnyAsync(x => x.Id == propertyId && x.Status != "Deleted" && x.Organization.OwnerAccountId == Owner, ct)) return NotFound();
        var counts = await db.Rooms.Where(x => x.PropertyId == propertyId && !x.IsDeleted)
            .GroupBy(x => x.Status).Select(x => new { Status = x.Key, Count = x.Count() }).ToListAsync(ct);
        var result = counts.ToDictionary(x => x.Status, x => x.Count);
        result["All"] = counts.Sum(x => x.Count);
        return Ok(result);
    }
    [HttpGet("api/contracts/summary")]
    public async Task<IActionResult> Contracts(Guid organizationId, CancellationToken ct)
    {
        if (!await db.Organizations.AnyAsync(x => x.Id == organizationId && x.OwnerAccountId == Owner, ct)) return NotFound();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); var soon = today.AddDays(30);
        var result = await db.Contracts.Where(x => x.OrganizationId == organizationId).GroupBy(x => x.OrganizationId)
            .Select(g => new { Total = g.Count(), Active = g.Count(x => x.Status == "Active" && x.EndDate > soon),
                Expiring = g.Count(x => x.Status == "Active" && x.EndDate >= today && x.EndDate <= soon),
                Overdue = g.Count(x => x.Status == "Active" && x.EndDate < today), Terminated = g.Count(x => x.Status == "Terminated") }).SingleOrDefaultAsync(ct);
        return Ok((object?)result ?? new { Total = 0, Active = 0, Expiring = 0, Overdue = 0, Terminated = 0 });
    }
}
