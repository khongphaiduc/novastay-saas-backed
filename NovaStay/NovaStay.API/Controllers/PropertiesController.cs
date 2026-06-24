using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

[Authorize]
[ApiController]
[Route("api/organizations/{organizationId:guid}/properties")]
public sealed class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertyDto>>> GetProperties(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var properties = await _propertyService.GetPropertiesByOrganizationAsync(organizationId, cancellationToken);
        return Ok(properties);
    }
}
