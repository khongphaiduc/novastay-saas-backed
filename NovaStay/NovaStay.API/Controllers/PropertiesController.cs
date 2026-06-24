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
