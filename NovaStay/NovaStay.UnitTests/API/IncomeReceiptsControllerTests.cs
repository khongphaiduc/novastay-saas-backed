using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.UnitTests.API;

public sealed class IncomeReceiptsControllerTests
{
    [Fact]
    public async Task GetById_ReturnsOk_WhenIncomeReceiptExists()
    {
        var receipt = new IncomeReceiptDto
        {
            Id = Guid.NewGuid(),
            ReceiptNumber = "PT001"
        };

        var service = new FakeIncomeReceiptService
        {
            ReceiptDetail = receipt
        };

        var controller = new IncomeReceiptsController(service);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.GetById(organizationId, propertyId, receipt.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(receipt, ok.Value);
        Assert.Equal(organizationId, service.DetailOrganizationId);
        Assert.Equal(propertyId, service.DetailPropertyId);
        Assert.Equal(receipt.Id, service.DetailIncomeReceiptId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenIncomeReceiptDoesNotExist()
    {
        var controller = new IncomeReceiptsController(new FakeIncomeReceiptService
        {
            ReceiptDetail = null
        });

        var result = await controller.GetById(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenPendingIncomeReceiptIsUpdated()
    {
        var receipt = new IncomeReceiptDto
        {
            Id = Guid.NewGuid(),
            Status = ApprovalStatus.Approved
        };

        var service = new FakeIncomeReceiptService
        {
            UpdateStatusResponse = receipt
        };

        var controller = new IncomeReceiptsController(service);
        var request = new UpdateApprovalStatusRequest { Status = ApprovalStatus.Approved };
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.UpdateStatus(organizationId, propertyId, receipt.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(receipt, ok.Value);
        Assert.Equal(receipt.Id, service.UpdateStatusIncomeReceiptId);
        Assert.Same(request, service.UpdateStatusRequest);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsBadRequest_WhenStatusCannotBeUpdated()
    {
        var controller = new IncomeReceiptsController(new FakeIncomeReceiptService
        {
            UpdateStatusException = new InvalidOperationException("Only income receipts with Pending status can be updated.")
        });

        var result = await controller.UpdateStatus(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdateApprovalStatusRequest { Status = ApprovalStatus.Rejected });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
