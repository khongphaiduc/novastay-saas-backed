using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleDataController : ControllerBase
{
    private readonly ISampleDataService _sampleDataService;

    public SampleDataController(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    [HttpGet("organizations")]
    public async Task<ActionResult<IReadOnlyList<OrganizationDto>>> GetOrganizations(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var organizations = await _sampleDataService.GetOrganizationsAsync(take, cancellationToken);

        return Ok(organizations);
    }
}
