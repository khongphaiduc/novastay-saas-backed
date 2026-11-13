using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class RemoveResidentTests
{
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _residentId = Guid.NewGuid();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IContractRepository> _contracts = new();
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly Mock<IResidentMembershipRepository> _memberships = new();
    private readonly ResidentMembershipEntity _membership;

    public RemoveResidentTests()
    {
        _membership = new ResidentMembershipEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = _orgId,
            ResidentId = _residentId,
            Status = new Status("ACTIVE"),
        };

        _memberships
            .Setup(m => m.GetByResidentAndOrganizationAsync(_residentId, _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_membership);
        _unitOfWork.SetupGet(u => u.Contracts).Returns(_contracts.Object);
        _unitOfWork.SetupGet(u => u.Invoices).Returns(_invoices.Object);
        _unitOfWork.SetupGet(u => u.ResidentMemberships).Returns(_memberships.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private OrganizationResidentService CreateService() =>
        new(new Mock<IOrganizationRepository>().Object, _memberships.Object, _unitOfWork.Object);

    [Fact]
    public async Task Remove_BlockedWhenResidentIsRepresentativeOfActiveContract()
    {
        _contracts.Setup(c => c.FindActiveContractOfResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractDetailDto
            {
                ResidentId = _residentId,
                RoomNumber = "101",
                EndDate = new DateOnly(2027, 10, 31),
                Status = "Active",
            });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().RemoveResidentAsync(_orgId, _residentId));

        Assert.Contains("đang là người đại diện", ex.Message);
        Assert.Contains("phòng 101", ex.Message);
        Assert.Contains("31/10/2027", ex.Message);
        Assert.Equal("ACTIVE", _membership.Status.Value);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_BlockedWhenResidentIsCoResidentOfActiveContract()
    {
        _contracts.Setup(c => c.FindActiveContractOfResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractDetailDto
            {
                ResidentId = Guid.NewGuid(),
                RoomNumber = "202",
                EndDate = new DateOnly(2027, 1, 15),
                Status = "Active",
            });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().RemoveResidentAsync(_orgId, _residentId));

        Assert.Contains("đang ở ghép", ex.Message);
        Assert.Contains("phòng 202", ex.Message);
        Assert.Equal("ACTIVE", _membership.Status.Value);
    }

    [Fact]
    public async Task Remove_BlockedWhenResidentHasUnpaidInvoices()
    {
        _contracts.Setup(c => c.FindActiveContractOfResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDetailDto?)null);
        _invoices.Setup(i => i.GetOpenInvoiceSummaryForResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 1_500_000m));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().RemoveResidentAsync(_orgId, _residentId));

        Assert.Contains("còn 2 hóa đơn", ex.Message);
        Assert.Contains("1.500.000 đ", ex.Message);
        Assert.Equal("ACTIVE", _membership.Status.Value);
    }

    [Fact]
    public async Task Remove_SucceedsAndMarksMembershipInactiveWhenNoConstraints()
    {
        _contracts.Setup(c => c.FindActiveContractOfResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDetailDto?)null);
        _invoices.Setup(i => i.GetOpenInvoiceSummaryForResidentAsync(_orgId, _residentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0m));

        await CreateService().RemoveResidentAsync(_orgId, _residentId);

        Assert.Equal("INACTIVE", _membership.Status.Value);
        _memberships.Verify(m => m.Update(_membership), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remove_ThrowsNotFoundWhenResidentNotInOrganization()
    {
        _memberships
            .Setup(m => m.GetByResidentAndOrganizationAsync(_residentId, _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResidentMembershipEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().RemoveResidentAsync(_orgId, _residentId));
    }
}
