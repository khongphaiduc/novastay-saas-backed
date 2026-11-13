using Moq;
using NovaStay.Application.Common;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

// Cư dân đã bị xóa khỏi cơ sở (membership INACTIVE) phải được mời lại được, dùng lại đúng dòng membership cũ.
public sealed class ReinviteResidentTests
{
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _residentId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IResidentMembershipRepository> _memberships = new();
    private readonly Mock<IOrganizationRepository> _organizations = new();
    private readonly Mock<IResidentRepository> _residents = new();

    public ReinviteResidentTests()
    {
        _organizations.Setup(o => o.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationEntity { Id = _orgId });
        _residents.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResidentEntity { Id = _residentId, AccountId = _accountId });

        _unitOfWork.SetupGet(u => u.Organizations).Returns(_organizations.Object);
        _unitOfWork.SetupGet(u => u.Residents).Returns(_residents.Object);
        _unitOfWork.SetupGet(u => u.ResidentMemberships).Returns(_memberships.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private ResidentMembershipEntity ExistingMembership(string status) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = _accountId,
        ResidentId = _residentId,
        OrganizationId = _orgId,
        MembershipCode = "NS-OLD",
        Status = new Status(status),
        JoinedAt = DateTime.UtcNow.AddDays(-30),
        ActivatedAt = DateTime.UtcNow.AddDays(-30),
    };

    private OrganizationResidentService CreateService() =>
        new(new Mock<IOrganizationRepository>().Object, _memberships.Object, _unitOfWork.Object);

    [Theory]
    [InlineData("INACTIVE")]
    [InlineData("REJECTED")]
    public async Task Invite_ReusesRemovedOrRejectedMembershipAndSetsPending(string previousStatus)
    {
        var membership = ExistingMembership(previousStatus);
        _memberships.Setup(m => m.GetByResidentAndOrganizationAsync(_residentId, _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        await CreateService().InviteResidentAsync(_orgId, new InviteResidentToOrganizationRequest { ResidentId = _residentId });

        Assert.Equal(ResidentMembershipStatuses.Pending, membership.Status.Value);
        Assert.Null(membership.JoinedAt);
        Assert.Null(membership.ActivatedAt);
        Assert.Equal("NS-OLD", membership.MembershipCode);
        _memberships.Verify(m => m.Update(membership), Times.Once);
        _memberships.Verify(m => m.AddAsync(It.IsAny<ResidentMembershipEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Invite_BlockedWhenResidentIsAlreadyActiveMember()
    {
        var membership = ExistingMembership(ResidentMembershipStatuses.Active);
        _memberships.Setup(m => m.GetByResidentAndOrganizationAsync(_residentId, _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().InviteResidentAsync(_orgId, new InviteResidentToOrganizationRequest { ResidentId = _residentId }));

        Assert.Contains("đang hoạt động", ex.Message);
        Assert.Equal(ResidentMembershipStatuses.Active, membership.Status.Value);
        _memberships.Verify(m => m.Update(It.IsAny<ResidentMembershipEntity>()), Times.Never);
    }
}
