using AutoMapper;
using MassTransit;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class AuditContractRulesTests
{
    [Theory]
    [InlineData(0, 10)] [InlineData(1, 0)] [InlineData(1, -1)] [InlineData(1, 201)] [InlineData(int.MaxValue, 200)]
    public async Task PagingBounds_AreRejectedBeforeRepositoryAccess(int page, int size)
    {
        var work = Mock.Of<IUnitOfWork>();
        var rooms = new RoomService(work, Mock.Of<IMapper>(), Mock.Of<IMinioStorageService>());
        var contracts = new ContractService(work, Mock.Of<IMapper>(), Mock.Of<IPublishEndpoint>());
        await Assert.ThrowsAsync<ArgumentException>(() => rooms.GetRoomsAsync(Guid.NewGuid(), pageIndex: page, pageSize: size));
        await Assert.ThrowsAsync<ArgumentException>(() => contracts.GetContractsAsync(Guid.NewGuid(), pageIndex: page, pageSize: size));
    }
    [Fact]
    public async Task UpdateDate_ValidatesFinalRangeWithoutSaving()
    {
        var id = Guid.NewGuid(); var work = new Mock<IUnitOfWork>();
        var lease = new ContractEntity { Id = id, StartDate = new(2026,1,1), EndDate = new(2026,12,31), Status = new Status("Active") };
        work.Setup(x => x.Contracts.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lease);
        var service = new ContractService(work.Object, Mock.Of<IMapper>(), Mock.Of<IPublishEndpoint>());
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractAsync(id, new UpdateContractRequest { StartDate = new(2027,1,1) }));
        Assert.Equal(new DateOnly(2026,1,1), lease.StartDate);
        work.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task RenewalOfTerminatedContract_CannotReoccupyRoom()
    {
        var id = Guid.NewGuid(); var work = new Mock<IUnitOfWork>();
        var lease = new ContractEntity { Id = id, EndDate = new(2026,12,31), Status = new Status("Terminated") };
        work.Setup(x => x.Contracts.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lease);
        var service = new ContractService(work.Object, Mock.Of<IMapper>(), Mock.Of<IPublishEndpoint>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RenewContractAsync(id, new RenewContractRequest { NewEndDate = new(2027,12,31) }));
        work.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
