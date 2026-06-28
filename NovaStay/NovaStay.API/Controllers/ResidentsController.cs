using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.API.Controllers;

[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/residents")]
public sealed class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;
    private readonly IMinioStorageService _storageService;

    public ResidentsController(IResidentService residentService, IMinioStorageService storageService)
    {
        _residentService = residentService;
        _storageService = storageService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateResidentAccountResponse>> CreateResidentAccount(
        [FromBody] CreateResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _residentService.CreateResidentAccountAsync(
                request,
                cancellationToken);

            return Created($"/api/residents/{response.ResidentId}", response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ResidentDto>>> SearchByPhone(
        [FromQuery] string? phone,
        CancellationToken cancellationToken = default)
    {
        var residents = await _residentService.SearchByPhoneAsync(phone, cancellationToken);

        return Ok(residents);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ResidentDto>> UpdateResident(
        Guid id,
        [FromBody] UpdateResidentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resident = await _residentService.UpdateResidentAsync(id, request, cancellationToken);
            return Ok(resident);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/images/{imageType}")]
    public async Task<ActionResult<ResidentDto>> UploadImage(
        Guid id,
        string imageType,
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _storageService.UploadImageAsync(stream, file.FileName, file.ContentType, cancellationToken);
            var resident = await _residentService.UploadImageAsync(id, imageType, imageUrl, cancellationToken);
            return Ok(resident);
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
}
