using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class FinancialTransactionsControllerTests
{
    [Fact]
    public async Task GetByProperty_ReturnsOk_WhenPropertyExists()
    {
        var pagedResult = new PagedResult<FinancialTransactionDto>
        {
            Items =
            [
                new FinancialTransactionDto
                {
                    Id = Guid.NewGuid(),
                    TransactionType = "Expense",
                    ReferenceNumber = "CP001"
                }
            ],
            TotalCount = 1,
            PageIndex = 2,
            PageSize = 5
        };

        var service = new FakeFinancialTransactionService
        {
            Result = pagedResult
        };

        var controller = new FinancialTransactionsController(service);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.GetByProperty(
            organizationId,
            propertyId,
            transactionType: "expense",
            search: "cp001",
            fromDate: new DateTime(2026, 6, 1),
            toDate: new DateTime(2026, 6, 30),
            pageIndex: 2,
            pageSize: 5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(pagedResult, ok.Value);
        Assert.Equal(organizationId, service.OrganizationId);
        Assert.Equal(propertyId, service.PropertyId);
        Assert.Equal("expense", service.TransactionType);
        Assert.Equal("cp001", service.Search);
        Assert.Equal(new DateTime(2026, 6, 1), service.FromDate);
        Assert.Equal(new DateTime(2026, 6, 30), service.ToDate);
        Assert.Equal(2, service.PageIndex);
        Assert.Equal(5, service.PageSize);
    }

    [Fact]
    public async Task GetByProperty_ReturnsNotFound_WhenPropertyDoesNotExist()
    {
        var controller = new FinancialTransactionsController(new FakeFinancialTransactionService
        {
            Result = null
        });

        var result = await controller.GetByProperty(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByProperty_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        var controller = new FinancialTransactionsController(new FakeFinancialTransactionService
        {
            Exception = new ArgumentException("pageSize must be greater than 0.")
        });

        var result = await controller.GetByProperty(Guid.NewGuid(), Guid.NewGuid(), pageSize: 0);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByProperty_ReturnsBadRequest_WhenDateRangeIsInvalid()
    {
        var controller = new FinancialTransactionsController(new FakeFinancialTransactionService
        {
            Exception = new ArgumentException("fromDate must be less than or equal to toDate.")
        });

        var result = await controller.GetByProperty(
            Guid.NewGuid(),
            Guid.NewGuid(),
            fromDate: new DateTime(2026, 6, 30),
            toDate: new DateTime(2026, 6, 1));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
