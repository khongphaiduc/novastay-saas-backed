using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

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
}
