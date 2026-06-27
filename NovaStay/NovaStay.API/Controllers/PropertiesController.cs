using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/organizations/{organizationId:guid}/properties")]
public sealed class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyCatalogService _propertyCatalogService;

    public PropertiesController(
        IPropertyService propertyService,
        IPropertyCatalogService propertyCatalogService)
    {
        _propertyService = propertyService;
        _propertyCatalogService = propertyCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropertyDto>>> GetProperties(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var properties = await _propertyService.GetPropertiesByOrganizationAsync(organizationId, cancellationToken);
        return Ok(properties);
    }

    [HttpGet("{propertyId:guid}/services")]
    public async Task<ActionResult<IReadOnlyList<PropertyServiceDto>>> GetPropertyServices(
        Guid organizationId,
        Guid propertyId,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _propertyCatalogService.GetPropertyServicesAsync(
            organizationId,
            propertyId,
            search,
            cancellationToken);

        if (response is null)
        {
            return NotFound(new { message = "Property not found in organization." });
        }

        return Ok(response);
    }

    [HttpPost("{propertyId:guid}/services")]
    public async Task<ActionResult<PropertyServiceDto>> CreatePropertyService(
        Guid organizationId,
        Guid propertyId,
        [FromBody] CreatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _propertyCatalogService.CreatePropertyServiceAsync(
                organizationId,
                propertyId,
                request,
                cancellationToken);

            return Created(
                $"/api/organizations/{organizationId}/properties/{propertyId}/services/{response.Id}",
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{propertyId:guid}/services/{propertyServiceId:guid}")]
    public async Task<ActionResult<PropertyServiceDto>> UpdatePropertyService(
        Guid organizationId,
        Guid propertyId,
        Guid propertyServiceId,
        [FromBody] UpdatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _propertyCatalogService.UpdatePropertyServiceAsync(
                organizationId,
                propertyId,
                propertyServiceId,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PropertyDto>> CreateProperty(
        Guid organizationId,
        [FromBody] CreatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var property = await _propertyService.CreatePropertyAsync(organizationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetProperties), new { organizationId }, property);
    }

    [HttpPut("{propertyId:guid}")]
    public async Task<ActionResult<PropertyDto>> UpdateProperty(
        Guid organizationId,
        Guid propertyId,
        [FromBody] UpdatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var property = await _propertyService.UpdatePropertyAsync(organizationId, propertyId, request, cancellationToken);
            return Ok(property);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{propertyId:guid}")]
    public async Task<ActionResult> DeleteProperty(
        Guid organizationId,
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _propertyService.DeletePropertyAsync(organizationId, propertyId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
