using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>[Admin] Thống kê tổng quan hệ thống</summary>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats(CancellationToken cancellationToken = default)
    {
        var stats = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>[Admin] Danh sách doanh nghiệp với search + status filter</summary>
    [HttpGet("organizations")]
    public async Task<ActionResult<IReadOnlyList<AdminOrganizationSummaryDto>>> GetOrganizations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var organizations = await _adminService.GetOrganizationSummariesAsync(page, pageSize, search, status, cancellationToken);
        return Ok(organizations);
    }

    /// <summary>[Admin] Danh sách cư dân toàn hệ thống (1 row/người) với search + status filter</summary>
    [HttpGet("residents")]
    public async Task<ActionResult<IReadOnlyList<AdminResidentSummaryDto>>> GetResidents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var residents = await _adminService.GetResidentSummariesAsync(page, pageSize, search, status, cancellationToken);
        return Ok(residents);
    }
}
