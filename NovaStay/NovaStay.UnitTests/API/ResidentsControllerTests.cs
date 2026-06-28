using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class ResidentsControllerTests
{
    [Fact]
    public async Task CreateResidentAccount_ReturnsCreated_WhenServiceSucceeds()
    {
        var response = new CreateResidentAccountResponse { ResidentId = Guid.NewGuid() };
        var controller = new ResidentsController(new FakeResidentService { CreateResponse = response }, null);

        var result = await controller.CreateResidentAccount(new CreateResidentAccountRequest());

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/residents/{response.ResidentId}", created.Location);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task CreateResidentAccount_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var controller = new ResidentsController(new FakeResidentService
        {
            CreateException = new InvalidOperationException("invalid")
        }, null);

        var result = await controller.CreateResidentAccount(new CreateResidentAccountRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task SearchByPhone_ReturnsOk()
    {
        var service = new FakeResidentService
        {
            SearchResults = [new ResidentDto { Id = Guid.NewGuid() }]
        };
        var controller = new ResidentsController(service, null);

        var result = await controller.SearchByPhone("090");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.SearchResults, ok.Value);
        Assert.Equal("090", service.SearchPhone);
    }
}
