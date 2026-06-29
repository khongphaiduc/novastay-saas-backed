using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.API.Controllers;

//[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/organizations/{organizationId:guid}/properties/{propertyId:guid}/income-receipts")]
public sealed class IncomeReceiptsController : ControllerBase
{
    private readonly IIncomeReceiptService _incomeReceiptService;

    public IncomeReceiptsController(IIncomeReceiptService incomeReceiptService)
    {
        _incomeReceiptService = incomeReceiptService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IncomeReceiptDto>>> GetByProperty(
        Guid organizationId,
        Guid propertyId,
        [FromQuery] ApprovalStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        var receipts = await _incomeReceiptService.GetByPropertyAsync(
            organizationId,
            propertyId,
            status,
            paymentMethod,
            cancellationToken);

        if (receipts is null)
        {
            return NotFound(new { message = "Property not found in organization." });
        }

        return Ok(receipts);
    }

    [HttpPost]
    public async Task<ActionResult<IncomeReceiptDto>> Create(
        Guid organizationId,
        Guid propertyId,
        [FromBody] CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var receipt = await _incomeReceiptService.CreateAsync(
                organizationId,
                propertyId,
                request,
                cancellationToken);

            return Created(
                $"/api/organizations/{organizationId}/properties/{propertyId}/income-receipts/{receipt.Id}",
                receipt);
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
}
