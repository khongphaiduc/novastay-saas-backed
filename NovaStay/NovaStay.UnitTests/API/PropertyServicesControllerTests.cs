using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class PropertyServicesControllerTests
{
    [Fact]
    public async Task GetPropertyServices_ReturnsOk_WhenPropertyExists()
    {
        var services = new List<PropertyServiceDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PropertyId = Guid.NewGuid(),
                ServiceName = "Dien"
            }
        };

        var catalogService = new FakePropertyCatalogService
        {
            Services = services
        };

        var controller = new PropertiesController(
            new FakePropertyService(),
            catalogService);

        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await controller.GetPropertyServices(organizationId, propertyId, "Dien");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(services, ok.Value);
        Assert.Equal(organizationId, catalogService.OrganizationId);
        Assert.Equal(propertyId, catalogService.PropertyId);
        Assert.Equal("Dien", catalogService.Search);
    }

    [Fact]
    public async Task GetPropertyServices_ReturnsNotFound_WhenPropertyDoesNotExist()
    {
        var controller = new PropertiesController(
            new FakePropertyService(),
            new FakePropertyCatalogService { Services = null });

        var result = await controller.GetPropertyServices(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreatePropertyService_ReturnsCreated_WhenServiceSucceeds()
    {
        var response = new PropertyServiceDto
        {
            Id = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            ServiceName = "Wifi",
            DefaultPrice = 200000
        };

        var catalogService = new FakePropertyCatalogService
        {
            CreateResponse = response
        };

        var controller = new PropertiesController(
            new FakePropertyService(),
            catalogService);

        var organizationId = Guid.NewGuid();
        var propertyId = response.PropertyId;
        var request = new CreatePropertyServiceRequest
        {
            ServiceName = "Wifi",
            DefaultPrice = 200000,
            BillingCycle = "Monthly",
            IsActive = true
        };

        var result = await controller.CreatePropertyService(
            organizationId,
            propertyId,
            request);

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/organizations/{organizationId}/properties/{propertyId}/services/{response.Id}", created.Location);
        Assert.Same(response, created.Value);
        Assert.Equal(organizationId, catalogService.OrganizationId);
        Assert.Equal(propertyId, catalogService.PropertyId);
        Assert.Same(request, catalogService.CreateRequest);
    }

    [Fact]
    public async Task CreatePropertyService_ReturnsNotFound_WhenPropertyDoesNotExist()
    {
        var controller = new PropertiesController(
            new FakePropertyService(),
            new FakePropertyCatalogService
            {
                CreateNotFoundException = new KeyNotFoundException("missing")
            });

        var result = await controller.CreatePropertyService(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreatePropertyServiceRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreatePropertyService_ReturnsBadRequest_WhenRequestIsRejected()
    {
        var controller = new PropertiesController(
            new FakePropertyService(),
            new FakePropertyCatalogService
            {
                CreateInvalidOperationException = new InvalidOperationException("duplicate")
            });

        var result = await controller.CreatePropertyService(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreatePropertyServiceRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdatePropertyService_ReturnsOk_WhenServiceSucceeds()
    {
        var response = new PropertyServiceDto
        {
            Id = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            ServiceName = "Dien",
            DefaultPrice = 5000,
            BillingCycle = "Monthly",
            IsActive = true
        };

        var catalogService = new FakePropertyCatalogService
        {
            UpdateResponse = response
        };

        var controller = new PropertiesController(
            new FakePropertyService(),
            catalogService);

        var organizationId = Guid.NewGuid();
        var propertyId = response.PropertyId;
        var propertyServiceId = response.Id;
        var request = new UpdatePropertyServiceRequest
        {
            DefaultPrice = 5000,
            BillingCycle = "Monthly",
            IsActive = true
        };

        var result = await controller.UpdatePropertyService(
            organizationId,
            propertyId,
            propertyServiceId,
            request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.Equal(organizationId, catalogService.OrganizationId);
        Assert.Equal(propertyId, catalogService.PropertyId);
        Assert.Equal(propertyServiceId, catalogService.PropertyServiceId);
        Assert.Same(request, catalogService.UpdateRequest);
    }

    [Fact]
    public async Task UpdatePropertyService_ReturnsNotFound_WhenTargetDoesNotExist()
    {
        var controller = new PropertiesController(
            new FakePropertyService(),
            new FakePropertyCatalogService
            {
                UpdateException = new KeyNotFoundException("missing")
            });

        var result = await controller.UpdatePropertyService(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdatePropertyServiceRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdatePropertyService_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var controller = new PropertiesController(
            new FakePropertyService(),
            new FakePropertyCatalogService
            {
                UpdateArgumentException = new ArgumentException("invalid")
            });

        var result = await controller.UpdatePropertyService(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdatePropertyServiceRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
