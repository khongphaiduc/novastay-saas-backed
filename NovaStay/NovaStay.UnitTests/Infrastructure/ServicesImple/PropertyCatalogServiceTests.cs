using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class PropertyCatalogServiceTests
{
    [Fact]
    public async Task GetPropertyServicesAsync_ShouldDelegateToRepository()
    {
        IReadOnlyList<PropertyServiceDto> expected =
        [
            new PropertyServiceDto
            {
                Id = Guid.NewGuid(),
                ServiceName = "Nuoc"
            }
        ];

        var repository = new Mock<IPropertyServiceRepository>();
        repository
            .Setup(instance => instance.GetPropertyServicesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new PropertyCatalogService(repository.Object);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        const string search = "dien";

        var result = await service.GetPropertyServicesAsync(organizationId, propertyId, $"  {search}  ");

        Assert.Same(expected, result);
        repository.Verify(instance => instance.GetPropertyServicesAsync(
            organizationId,
            propertyId,
            search,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePropertyServiceAsync_ShouldDelegateToRepository()
    {
        var expected = new PropertyServiceDto
        {
            Id = Guid.NewGuid(),
            ServiceName = "Wifi",
            DefaultPrice = 200000
        };

        var repository = new Mock<IPropertyServiceRepository>();
        repository
            .Setup(instance => instance.CreatePropertyServiceAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CreatePropertyServiceRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new PropertyCatalogService(repository.Object);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var request = new CreatePropertyServiceRequest
        {
            ServiceName = "  Wifi  ",
            ServiceCode = "  WF  ",
            Description = "  Internet  ",
            DefaultPrice = 200000,
            Unit = "  package  ",
            BillingCycle = "  Monthly  ",
            IsActive = true
        };

        var result = await service.CreatePropertyServiceAsync(organizationId, propertyId, request);

        Assert.Same(expected, result);
        Assert.Equal("Wifi", request.ServiceName);
        Assert.Equal("WF", request.ServiceCode);
        Assert.Equal("Internet", request.Description);
        Assert.Equal("package", request.Unit);
        Assert.Equal("Monthly", request.BillingCycle);
        repository.Verify(instance => instance.CreatePropertyServiceAsync(
            organizationId,
            propertyId,
            request,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePropertyServiceAsync_ShouldThrowArgumentException_WhenServiceNameIsMissing()
    {
        var repository = new Mock<IPropertyServiceRepository>();
        var service = new PropertyCatalogService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePropertyServiceAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreatePropertyServiceRequest
            {
                ServiceName = " ",
                DefaultPrice = 1000
            }));

        repository.Verify(instance => instance.CreatePropertyServiceAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CreatePropertyServiceRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePropertyServiceAsync_ShouldThrowArgumentException_WhenPriceIsNegative()
    {
        var repository = new Mock<IPropertyServiceRepository>();
        var service = new PropertyCatalogService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePropertyServiceAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreatePropertyServiceRequest
            {
                ServiceName = "Wifi",
                DefaultPrice = -1
            }));

        repository.Verify(instance => instance.CreatePropertyServiceAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CreatePropertyServiceRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePropertyServiceAsync_ShouldDelegateToRepository()
    {
        var expected = new PropertyServiceDto
        {
            Id = Guid.NewGuid(),
            ServiceName = "Nuoc",
            DefaultPrice = 15000,
            IsActive = false
        };

        var repository = new Mock<IPropertyServiceRepository>();
        repository
            .Setup(instance => instance.UpdatePropertyServiceAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new PropertyCatalogService(repository.Object);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var propertyServiceId = Guid.NewGuid();
        var request = new UpdatePropertyServiceRequest
        {
            DefaultPrice = 15000,
            BillingCycle = "Monthly",
            IsActive = false
        };

        var result = await service.UpdatePropertyServiceAsync(
            organizationId,
            propertyId,
            propertyServiceId,
            request);

        Assert.Same(expected, result);
        repository.Verify(instance => instance.UpdatePropertyServiceAsync(
            organizationId,
            propertyId,
            propertyServiceId,
            request.DefaultPrice,
            request.BillingCycle,
            request.IsActive,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePropertyServiceAsync_ShouldThrowArgumentException_WhenPriceIsNegative()
    {
        var repository = new Mock<IPropertyServiceRepository>();
        var service = new PropertyCatalogService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePropertyServiceAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdatePropertyServiceRequest
            {
                DefaultPrice = -1,
                IsActive = true
            }));

        repository.Verify(instance => instance.UpdatePropertyServiceAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<string?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
