using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.API.Security;

// Runs after binding but before action code (including uploads) can cause side effects.
public sealed class OwnerDataAttribute() : TypeFilterAttribute(typeof(OwnerDataFilter));

public sealed class OwnerDataFilter(HostContext db) : IAsyncActionFilter
{
    // Fields that carry no tenant resource: server-derived identity or global catalog values (not client-trusted).
    private static readonly HashSet<string> AllowedUnscoped = new(StringComparer.OrdinalIgnoreCase)
    { "id", "accountid", "owner", "owneraccountid", "packageid", "eventid", "attemptid", "currentassignmentid" };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (!user.IsInRole("BusinessOwner")) { await next(); return; }
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var owner))
        { context.Result = new UnauthorizedResult(); return; }
        var ct = context.HttpContext.RequestAborted;
        var controller = context.RouteData.Values["controller"]?.ToString();
        var fields = new List<KeyValuePair<string, Guid>>();
        foreach (var (name, argument) in context.ActionArguments)
        {
            if (argument is Guid id) fields.Add(new(name, id));
            else if (argument != null && argument.GetType().Namespace?.StartsWith("NovaStay.", StringComparison.Ordinal) == true)
                foreach (var property in argument.GetType().GetProperties())
                {
                    // Inviting a resident is explicitly allowed before they have a membership.
                    if (argument is NovaStay.Application.DTOs.InviteResidentToOrganizationRequest && property.Name == "ResidentId") continue;
                    if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;
                    var propertyValue = property.GetValue(argument);
                    if (propertyValue is Guid value) fields.Add(new(property.Name, value));
                    else if (propertyValue is IEnumerable<Guid> values)
                        foreach (var item in values) fields.Add(new(property.Name, item));
                }
        }
        if (context.ActionArguments.TryGetValue("id", out var argumentId) && argumentId is Guid routeId)
            fields.Add(new(controller switch { "Rooms" => "RoomId", "Contracts" => "ContractId", "Residents" => "ResidentId", "Assets" => "AssetId", "UtilityMeters" => "MeterId", "Maintenance" => "TicketId", _ => "id" }, routeId));
        var valid = true;
        foreach (var (name, id) in fields)
        {
            valid = name.ToLowerInvariant() switch
            {
                "organizationid" => await db.Organizations.AnyAsync(x => x.Id == id && x.OwnerAccountId == owner, ct),
                "propertyid" => await db.Properties.AnyAsync(x => x.Id == id && x.Status != "Deleted" && x.Organization.OwnerAccountId == owner, ct),
                "roomid" => await db.Rooms.AnyAsync(x => x.Id == id && !x.IsDeleted && x.Property.Status != "Deleted" && x.Property.Organization.OwnerAccountId == owner, ct),
                "contractid" => await db.Contracts.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "residentid" => await db.ResidentMemberships.AnyAsync(x => x.ResidentId == id && x.Organization.OwnerAccountId == owner && x.Status == "ACTIVE", ct),
                "expenseid" => await db.Expenses.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "incomereceiptid" => await db.IncomeReceipts.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "ticketid" => await db.MaintenanceTickets.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "categoryid" when controller == "Expenses" => await db.ExpenseCategories.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "categoryid" when controller == "IncomeReceipts" => await db.IncomeCategories.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "assetid" => await db.Assets.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "assignmentid" => await db.AssetAssignments.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "meterid" => await db.UtilityMeters.AnyAsync(x => x.Id == id && x.Room.Property.Organization.OwnerAccountId == owner, ct),
                "propertyserviceid" => await db.PropertyServices.AnyAsync(x => x.Id == id && x.Property.Organization.OwnerAccountId == owner, ct),
                "membershipid" => await db.ResidentMemberships.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "brokerid" or "relatedbrokerid" when id == Guid.Empty => true,
                "brokerid" or "relatedbrokerid" => await db.Brokers.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "incomecategoryid" => await db.IncomeCategories.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "expensecategoryid" => await db.ExpenseCategories.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "utilitymeterid" => await db.UtilityMeters.AnyAsync(x => x.Id == id && x.Room.Property.Organization.OwnerAccountId == owner, ct),
                "relatedmaintenanceticketid" => await db.MaintenanceTickets.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "assetassignmentid" => await db.AssetAssignments.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "invoiceid" => await db.Invoices.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "bookingid" => await db.Bookings.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "currentroomid" => await db.Rooms.AnyAsync(x => x.Id == id && x.Property.Organization.OwnerAccountId == owner, ct),
                "technicianid" => await db.Technicians.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                "staffuserid" or "createdbystaffuserid" or "collectedbystaffuserid" or "approvedbystaffuserid" =>
                    await db.StaffUsers.AnyAsync(x => x.Id == id && x.Organization.OwnerAccountId == owner, ct),
                // Scoped by a parent ID that is validated above (service queries by parent + child), or server-owned values.
                "linkid" or "imageid" => true,
                // Unknown ID field: fail closed so new DTO/route IDs cannot silently bypass ownership checks.
                _ => AllowedUnscoped.Contains(name)
            };
            if (!valid) break;
        }
        if (!valid) { context.Result = new NotFoundObjectResult(new { message = "Không tìm thấy dữ liệu thuộc quyền quản lý của bạn." }); return; }
        await next();
    }
}
