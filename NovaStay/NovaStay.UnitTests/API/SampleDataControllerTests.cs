using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class SampleDataControllerTests
{
    [Fact]
    public async Task GetOrganizations_ReturnsOk()
    {
        var service = new FakeSampleDataService
        {
            Organizations = [new OrganizationDto { Id = Guid.NewGuid() }]
        };
        var controller = new SampleDataController(service);

        var result = await controller.GetOrganizations(5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.Organizations, ok.Value);
        Assert.Equal(5, service.Take);
    }
}
