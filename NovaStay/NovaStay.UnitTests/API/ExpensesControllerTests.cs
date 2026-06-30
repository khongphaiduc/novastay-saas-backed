using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.UnitTests.API;

public sealed class ExpensesControllerTests
{
    [Fact]
    public async Task GetById_ReturnsOk_WhenExpenseExists()
    {
        var expense = new ExpenseDto
        {
            Id = Guid.NewGuid(),
            ExpenseNumber = "PC001"
        };

        var service = new FakeExpenseService
        {
            ExpenseDetail = expense
        };

        var controller = new ExpensesController(service);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.GetById(organizationId, propertyId, expense.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expense, ok.Value);
        Assert.Equal(organizationId, service.DetailOrganizationId);
        Assert.Equal(propertyId, service.DetailPropertyId);
        Assert.Equal(expense.Id, service.DetailExpenseId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenExpenseDoesNotExist()
    {
        var controller = new ExpensesController(new FakeExpenseService
        {
            ExpenseDetail = null
        });

        var result = await controller.GetById(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenPendingExpenseIsUpdated()
    {
        var expense = new ExpenseDto
        {
            Id = Guid.NewGuid(),
            Status = ApprovalStatus.Approved
        };

        var service = new FakeExpenseService
        {
            UpdateStatusResponse = expense
        };

        var controller = new ExpensesController(service);
        var request = new UpdateApprovalStatusRequest { Status = ApprovalStatus.Approved };
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.UpdateStatus(organizationId, propertyId, expense.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expense, ok.Value);
        Assert.Equal(expense.Id, service.UpdateStatusExpenseId);
        Assert.Same(request, service.UpdateStatusRequest);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsBadRequest_WhenStatusCannotBeUpdated()
    {
        var controller = new ExpensesController(new FakeExpenseService
        {
            UpdateStatusException = new InvalidOperationException("Only expenses with Pending status can be updated.")
        });

        var result = await controller.UpdateStatus(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdateApprovalStatusRequest { Status = ApprovalStatus.Rejected });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
