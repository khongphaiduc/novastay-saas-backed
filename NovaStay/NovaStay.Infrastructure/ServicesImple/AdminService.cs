using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class AdminService : IAdminService
{
    private readonly HostContext _context;

    public AdminService(HostContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var accountCounts = await _context.Accounts
            .AsNoTracking()
            .GroupBy(a => a.AccountType)
            .Select(g => new { AccountType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalOrganizations = await _context.Organizations
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalResidents = await _context.Residents
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var businessOwnerCount = accountCounts.FirstOrDefault(x => x.AccountType == "BusinessOwner")?.Count ?? 0;
        var residentAccountCount = accountCounts.FirstOrDefault(x => x.AccountType == "Resident")?.Count ?? 0;
        var totalAllAccounts = accountCounts.Sum(x => x.Count);

        return new AdminDashboardStatsDto
        {
            TotalOrganizations = totalOrganizations,
            TotalResidents = totalResidents,
            TotalBusinessOwnerAccounts = businessOwnerCount,
            TotalResidentAccounts = residentAccountCount,
            TotalAllAccounts = totalAllAccounts,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminOrganizationSummaryDto>> GetOrganizationSummariesAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var searchLower = search?.Trim().ToLowerInvariant();

        var result = await (
            from org in _context.Organizations.AsNoTracking()
            let residentCount = _context.ResidentMemberships
                .Count(rm => rm.OrganizationId == org.Id && rm.Status == "Active")
            where (searchLower == null
                   || org.BusinessName.ToLower().Contains(searchLower)
                   || org.BusinessArea.ToLower().Contains(searchLower)
                   || (org.OwnerEmail != null && org.OwnerEmail.ToLower().Contains(searchLower)))
            where (status == null || org.SubscriptionStatus == status)
            orderby org.CreatedAt descending
            select new AdminOrganizationSummaryDto
            {
                OrganizationId = org.Id,
                BusinessName = org.BusinessName,
                BusinessArea = org.BusinessArea,
                OwnerEmail = org.OwnerEmail,
                SubscriptionStatus = org.SubscriptionStatus,
                ResidentCount = residentCount,
                CreatedAt = org.CreatedAt,
            }
        )
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminResidentSummaryDto>> GetResidentSummariesAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var searchLower = search?.Trim().ToLowerInvariant();

        // Step 1: Get paged residents (1 row per resident, no join yet)
        var residentQuery = _context.Residents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchLower))
        {
            residentQuery = residentQuery.Where(r =>
                (r.FullName != null && r.FullName.ToLower().Contains(searchLower)) ||
                (r.Email != null && r.Email.ToLower().Contains(searchLower)) ||
                (r.Phone != null && r.Phone.Contains(searchLower)));
        }

        var pagedResidents = await residentQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new { r.Id, r.FullName, r.Email, r.Phone, r.CreatedAt })
            .ToListAsync(cancellationToken);

        if (pagedResidents.Count == 0)
            return Array.Empty<AdminResidentSummaryDto>();

        // Step 2: Get the LATEST membership per resident (avoid duplicates)
        var residentIds = pagedResidents.Select(r => r.Id).ToList();

        var latestMemberships = await _context.ResidentMemberships
            .AsNoTracking()
            .Where(rm => residentIds.Contains(rm.ResidentId)
                         && (status == null || rm.Status == status))
            .GroupBy(rm => rm.ResidentId)
            .Select(g => g.OrderByDescending(rm => rm.JoinedAt ?? rm.CreatedAt).First())
            .ToListAsync(cancellationToken);

        // Step 3: Get organization names for those memberships
        var orgIds = latestMemberships.Select(rm => rm.OrganizationId).Distinct().ToList();
        var orgNames = await _context.Organizations
            .AsNoTracking()
            .Where(o => orgIds.Contains(o.Id))
            .Select(o => new { o.Id, o.BusinessName })
            .ToDictionaryAsync(o => o.Id, o => o.BusinessName, cancellationToken);

        // Step 4: Build result dictionary keyed by ResidentId
        var membershipByResident = latestMemberships
            .ToDictionary(rm => rm.ResidentId);

        var results = pagedResidents.Select(r =>
        {
            membershipByResident.TryGetValue(r.Id, out var rm);
            var orgName = rm != null && orgNames.TryGetValue(rm.OrganizationId, out var name) ? name : string.Empty;
            return new AdminResidentSummaryDto
            {
                ResidentId = r.Id,
                FullName = r.FullName ?? string.Empty,
                Email = r.Email ?? string.Empty,
                Phone = r.Phone ?? string.Empty,
                OrganizationName = orgName,
                MembershipStatus = rm?.Status ?? string.Empty,
                JoinedAt = rm?.JoinedAt,
            };
        }).ToList();

        // If status filter applied, skip residents with no matching membership
        if (!string.IsNullOrWhiteSpace(status))
            results = results.Where(r => r.MembershipStatus == status).ToList();

        return results;
    }
}
