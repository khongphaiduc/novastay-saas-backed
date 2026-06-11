using Host.Application.DTOs;
using Host.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleDataController : ControllerBase
{
    private readonly ISampleDataService _sampleDataService;

    public SampleDataController(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> GetTenants(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var tenants = await _sampleDataService.GetTenantsAsync(take, cancellationToken);

        return Ok(tenants);
    }
}
