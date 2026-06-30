using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.API.Controllers;

[ApiController]
[Route("api/organizations/{organizationId:guid}/properties/{propertyId:guid}/financial-transactions")]
public sealed class FinancialTransactionsController : ControllerBase
{
    private readonly IFinancialTransactionService _financialTransactionService;

    public FinancialTransactionsController(IFinancialTransactionService financialTransactionService)
    {
        _financialTransactionService = financialTransactionService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<FinancialTransactionDto>>> GetByProperty(
        Guid organizationId,
        Guid propertyId,
        [FromQuery] string? transactionType = null,
        [FromQuery] string? search = null,
        [FromQuery] ApprovalStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _financialTransactionService.GetByPropertyAsync(
                organizationId,
                propertyId,
                transactionType,
                search,
                status,
                paymentMethod,
                fromDate,
                toDate,
                pageIndex,
                pageSize,
                cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = "Property not found in organization." });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
