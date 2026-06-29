using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.API.Controllers;

//[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/organizations/{organizationId:guid}/properties/{propertyId:guid}/expenses")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetByProperty(
        Guid organizationId,
        Guid propertyId,
        [FromQuery] ApprovalStatus? status = null,
        [FromQuery] PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseService.GetByPropertyAsync(
            organizationId,
            propertyId,
            status,
            paymentMethod,
            cancellationToken);

        if (expenses is null)
        {
            return NotFound(new { message = "Property not found in organization." });
        }

        return Ok(expenses);
    }

    [HttpGet("{expenseId:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(
        Guid organizationId,
        Guid propertyId,
        Guid expenseId,
        CancellationToken cancellationToken = default)
    {
        var expense = await _expenseService.GetByIdAsync(
            organizationId,
            propertyId,
            expenseId,
            cancellationToken);

        if (expense is null)
        {
            return NotFound(new { message = "Expense not found in property." });
        }

        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        Guid organizationId,
        Guid propertyId,
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expense = await _expenseService.CreateAsync(
                organizationId,
                propertyId,
                request,
                cancellationToken);

            return Created(
                $"/api/organizations/{organizationId}/properties/{propertyId}/expenses/{expense.Id}",
                expense);
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
