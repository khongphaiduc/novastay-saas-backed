using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple
{
    public class AssetServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAssetRepository> _mockAssetRepo;
        private readonly Mock<IAssetAssignmentRepository> _mockAssignmentRepo;
        private readonly AssetService _assetService;

        private readonly Guid _orgId = Guid.NewGuid();
        private readonly Guid _assetId = Guid.NewGuid();
        private readonly Guid _roomId = Guid.NewGuid();

        public AssetServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAssetRepo = new Mock<IAssetRepository>();
            _mockAssignmentRepo = new Mock<IAssetAssignmentRepository>();

            _mockUnitOfWork.Setup(u => u.Assets).Returns(_mockAssetRepo.Object);
            _mockUnitOfWork.Setup(u => u.AssetAssignments).Returns(_mockAssignmentRepo.Object);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _assetService = new AssetService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        #region TASK-055: GetAssetsAsync

        [Fact]
        public async Task GetAssetsAsync_WithValidOrgId_ReturnsAssets()
        {
            // Arrange
            var expectedAssets = new List<AssetDto>
            {
                new AssetDto { Id = Guid.NewGuid(), AssetName = "Máy lạnh", CurrentStatus = "Good" },
                new AssetDto { Id = Guid.NewGuid(), AssetName = "Tủ lạnh", CurrentStatus = "Damaged" }
            };

            _mockAssetRepo.Setup(r => r.GetAssetsWithCurrentStatusAsync(
                    _orgId, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAssets);

            // Act
            var result = await _assetService.GetAssetsAsync(_orgId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockAssetRepo.Verify(r => r.GetAssetsWithCurrentStatusAsync(
                _orgId, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAssetsAsync_WithEmptyOrgId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assetService.GetAssetsAsync(Guid.Empty));
        }

        #endregion

        #region TASK-056: CreateAssetAsync

        [Fact]
        public async Task CreateAssetAsync_WithValidData_CreatesAssetAndInitialAssignment()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                OrganizationId = _orgId,
                AssetName = "Điều hòa Samsung",
                Category = "Electronics",
                Brand = "Samsung",
                Model = "AS09A",
                AssetCode = "AC-001",
                BaseValue = 5000000,
                InitialNote = "Mua mới"
            };

            AssetEntity savedAsset = null!;
            AssetAssignmentEntity savedAssignment = null!;

            _mockAssetRepo.Setup(r => r.AddAsync(It.IsAny<AssetEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AssetEntity, CancellationToken>((a, c) => savedAsset = a)
                .Returns(Task.CompletedTask);

            _mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<AssetAssignmentEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AssetAssignmentEntity, CancellationToken>((a, c) => savedAssignment = a)
                .Returns(Task.CompletedTask);

            var returnedDtos = new List<AssetDto>
            {
                new AssetDto { Id = Guid.NewGuid(), AssetName = "Điều hòa Samsung" }
            };
            _mockAssetRepo.Setup(r => r.GetAssetsWithCurrentStatusAsync(
                    _orgId, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnedDtos);

            // Act
            await _assetService.CreateAssetAsync(request);

            // Assert
            Assert.NotNull(savedAsset);
            Assert.NotEqual(Guid.Empty, savedAsset.Id);
            Assert.Equal(_orgId, savedAsset.OrganizationId);
            Assert.Equal("Electronics", savedAsset.Category);

            Assert.NotNull(savedAssignment);
            Assert.Equal(savedAsset.Id, savedAssignment.AssetId);
            Assert.Null(savedAssignment.RoomId);       // Vào kho, chưa gán phòng
            Assert.Equal("Good", savedAssignment.Status);
            Assert.Equal("Mua mới", savedAssignment.Note);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAssetAsync_WithEmptyName_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                OrganizationId = _orgId,
                AssetName = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assetService.CreateAssetAsync(request));
        }

        [Fact]
        public async Task CreateAssetAsync_WithEmptyOrgId_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                OrganizationId = Guid.Empty,
                AssetName = "Tài sản test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assetService.CreateAssetAsync(request));
        }

        #endregion

        #region TASK-056: DeleteAssetAsync (Soft Delete)

        [Fact]
        public async Task DeleteAssetAsync_WithUnassignedAsset_SoftDeletes()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            // Tài sản không gán phòng nào (RoomId = null)
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetAssignmentEntity?)null);

            // Act
            await _assetService.DeleteAssetAsync(_assetId);

            // Assert
            Assert.True(existingAsset.IsDeleted);
            _mockAssetRepo.Verify(r => r.Update(existingAsset), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAssetAsync_WhenAssetAssignedToRoom_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };
            var currentAssignment = new AssetAssignmentEntity
            {
                Id = Guid.NewGuid(),
                AssetId = _assetId,
                RoomId = _roomId // Đang gán phòng
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentAssignment);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _assetService.DeleteAssetAsync(_assetId));

            // Không được gọi Update
            _mockAssetRepo.Verify(r => r.Update(It.IsAny<AssetEntity>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAssetAsync_WithNonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _assetService.DeleteAssetAsync(_assetId));
        }

        #endregion

        #region TASK-057: UpdateAssetAsync

        [Fact]
        public async Task UpdateAssetAsync_WithValidData_UpdatesAsset()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Cũ"),
                IsDeleted = false
            };
            var request = new UpdateAssetRequest
            {
                AssetName = "Điều hòa mới",
                Category = "Electronics",
                Brand = "LG",
                Model = "2024"
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            _mockAssetRepo.Setup(r => r.GetAssetsWithCurrentStatusAsync(
                    _orgId, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetDto>
                {
                    new AssetDto { Id = _assetId, AssetName = "Điều hòa mới" }
                });

            // Act
            var result = await _assetService.UpdateAssetAsync(_assetId, request);

            // Assert
            Assert.Equal("Điều hòa mới", existingAsset.AssetName.Value);
            Assert.Equal("Electronics", existingAsset.Category);
            _mockAssetRepo.Verify(r => r.Update(existingAsset), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region TASK-058: AssignAssetToRoomAsync

        [Fact]
        public async Task AssignAssetToRoomAsync_WhenAssetInStorage_CreatesAssignment()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };
            var request = new AssignAssetRequest
            {
                RoomId = _roomId,
                Note = "Lắp đặt phòng mới"
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            // Tài sản đang trong kho (RoomId = null)
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetAssignmentEntity?)null);

            AssetAssignmentEntity savedAssignment = null!;
            _mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<AssetAssignmentEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AssetAssignmentEntity, CancellationToken>((a, c) => savedAssignment = a)
                .Returns(Task.CompletedTask);

            _mockAssignmentRepo.Setup(r => r.GetHistoryByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetHistoryDto>
                {
                    new AssetHistoryDto { RoomId = _roomId, Status = "Good" }
                });

            // Act
            await _assetService.AssignAssetToRoomAsync(_assetId, request);

            // Assert
            Assert.NotNull(savedAssignment);
            Assert.Equal(_roomId, savedAssignment.RoomId);
            Assert.Equal(_assetId, savedAssignment.AssetId);
            Assert.Equal("Good", savedAssignment.Status);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignAssetToRoomAsync_WhenAlreadyAssignedToAnotherRoom_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };
            var otherRoomId = Guid.NewGuid();
            var currentAssignment = new AssetAssignmentEntity
            {
                AssetId = _assetId,
                RoomId = otherRoomId // Đang ở phòng khác
            };
            var request = new AssignAssetRequest { RoomId = _roomId };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentAssignment);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _assetService.AssignAssetToRoomAsync(_assetId, request));
        }

        #endregion

        #region TASK-059: RevokeAssetFromRoomAsync

        [Fact]
        public async Task RevokeAssetFromRoomAsync_WhenAssigned_CreatesRevocationRecord()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };
            var currentAssignment = new AssetAssignmentEntity
            {
                AssetId = _assetId,
                RoomId = _roomId,
                Status = "Good"
            };
            var request = new RevokeAssetRequest { Note = "Thu hồi kiểm tra" };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentAssignment);

            AssetAssignmentEntity savedRevoke = null!;
            _mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<AssetAssignmentEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AssetAssignmentEntity, CancellationToken>((a, c) => savedRevoke = a)
                .Returns(Task.CompletedTask);

            _mockAssignmentRepo.Setup(r => r.GetHistoryByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetHistoryDto>());

            // Act
            await _assetService.RevokeAssetFromRoomAsync(_assetId, request);

            // Assert
            Assert.NotNull(savedRevoke);
            Assert.Null(savedRevoke.RoomId);  // Đã về kho
            Assert.Equal("Thu hồi kiểm tra", savedRevoke.Note);
        }

        [Fact]
        public async Task RevokeAssetFromRoomAsync_WhenNotAssigned_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            // Tài sản đang trong kho
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetAssignmentEntity { RoomId = null });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _assetService.RevokeAssetFromRoomAsync(_assetId, new RevokeAssetRequest()));
        }

        #endregion

        #region TASK-060 & TASK-061: UpdateAssetStatusAsync

        [Fact]
        public async Task UpdateAssetStatusAsync_WithDamagedStatus_CreatesStatusRecord()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Tủ lạnh"),
                IsDeleted = false
            };
            var currentAssignment = new AssetAssignmentEntity
            {
                AssetId = _assetId,
                RoomId = _roomId,
                Status = "Good"
            };
            var request = new UpdateAssetStatusRequest
            {
                Status = "Damaged",
                Note = "Cánh cửa bị hỏng"
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);
            _mockAssignmentRepo.Setup(r => r.GetLatestByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentAssignment);

            AssetAssignmentEntity savedStatus = null!;
            _mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<AssetAssignmentEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AssetAssignmentEntity, CancellationToken>((a, c) => savedStatus = a)
                .Returns(Task.CompletedTask);

            _mockAssignmentRepo.Setup(r => r.GetHistoryByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetHistoryDto>());

            // Act
            await _assetService.UpdateAssetStatusAsync(_assetId, request);

            // Assert
            Assert.NotNull(savedStatus);
            Assert.Equal("Damaged", savedStatus.Status);
            Assert.Equal(_roomId, savedStatus.RoomId); // Giữ nguyên phòng
            Assert.Equal("Cánh cửa bị hỏng", savedStatus.Note);
        }

        [Fact]
        public async Task UpdateAssetStatusAsync_WithInvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            var request = new UpdateAssetStatusRequest { Status = "InvalidStatus" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assetService.UpdateAssetStatusAsync(_assetId, request));
        }

        #endregion

        #region TASK-062: GetAssetHistoryAsync

        [Fact]
        public async Task GetAssetHistoryAsync_ReturnsHistoryInDescendingOrder()
        {
            // Arrange
            var history = new List<AssetHistoryDto>
            {
                new AssetHistoryDto { Id = Guid.NewGuid(), Status = "Damaged", AssignedAt = DateTime.UtcNow },
                new AssetHistoryDto { Id = Guid.NewGuid(), Status = "Good", AssignedAt = DateTime.UtcNow.AddDays(-1) },
                new AssetHistoryDto { Id = Guid.NewGuid(), Status = "Good", AssignedAt = DateTime.UtcNow.AddDays(-30) },
            };

            var existingAsset = new AssetEntity
            {
                Id = _assetId,
                OrganizationId = _orgId,
                AssetName = new EntityName("Test"),
                IsDeleted = false
            };

            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAsset);

            _mockAssignmentRepo.Setup(r => r.GetHistoryByAssetAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Act
            var result = await _assetService.GetAssetHistoryAsync(_assetId);

            // Assert
            Assert.Equal(3, result.Count);
            _mockAssignmentRepo.Verify(r => r.GetHistoryByAssetAsync(_assetId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAssetHistoryAsync_WithNonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockAssetRepo.Setup(r => r.GetByIdAsync(_assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _assetService.GetAssetHistoryAsync(_assetId));
        }

        #endregion

        #region Derived Task: GetStatisticsAsync

        [Fact]
        public async Task GetStatisticsAsync_WithValidOrgId_ReturnsStatistics()
        {
            // Arrange
            var expectedStats = new AssetStatisticsDto
            {
                TotalAssets = 10,
                InStorage = 3,
                InUse = 5,
                Damaged = 1,
                InMaintenance = 1
            };

            _mockAssetRepo.Setup(r => r.GetStatisticsAsync(_orgId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _assetService.GetStatisticsAsync(_orgId);

            // Assert
            Assert.Equal(10, result.TotalAssets);
            Assert.Equal(3, result.InStorage);
            Assert.Equal(5, result.InUse);
            Assert.Equal(1, result.Damaged);
        }

        [Fact]
        public async Task GetStatisticsAsync_WithEmptyOrgId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assetService.GetStatisticsAsync(Guid.Empty));
        }

        #endregion
    }
}
