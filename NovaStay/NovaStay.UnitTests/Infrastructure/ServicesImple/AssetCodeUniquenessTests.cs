using AutoMapper;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

// Mã tài sản phải duy nhất trong mỗi cơ sở.
public sealed class AssetCodeUniquenessTests
{
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAssetRepository> _assets = new();
    private readonly Mock<IMapper> _mapper = new();

    public AssetCodeUniquenessTests()
    {
        _unitOfWork.SetupGet(u => u.Assets).Returns(_assets.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private AssetService CreateService() => new(_unitOfWork.Object, _mapper.Object);

    [Fact]
    public async Task Create_BlockedWhenAssetCodeAlreadyExistsInOrganization()
    {
        _assets.Setup(a => a.AssetCodeExistsAsync(_orgId, "AC-001", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().CreateAssetAsync(new CreateAssetRequest
        {
            OrganizationId = _orgId,
            AssetName = "Điều hòa",
            AssetCode = " AC-001 ",
        }));

        Assert.Contains("AC-001", ex.Message);
        Assert.Contains("đã tồn tại", ex.Message);
        _assets.Verify(a => a.AddAsync(It.IsAny<AssetEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
