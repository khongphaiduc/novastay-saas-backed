using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using NovaStay.Infrastructure.ServicesImple;
using MassTransit;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple
{
    public class ContractServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly ContractService _contractService;

        public ContractServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();

            _contractService = new ContractService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task CreateContractAsync_ShouldSaveAndReturnDto()
        {
            // Arrange
            var request = new CreateContractRequest
            {
                OrganizationId = Guid.NewGuid(),
                PropertyId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid(),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
                DepositAmount = 5000000
            };

            var roomEntity = new RoomEntity
            {
                Id = request.RoomId,
                RoomNumber = "101",
                Status = new Status("Available")
            };

            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(request.RoomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);

            ContractEntity savedContract = null!;
            _mockUnitOfWork.Setup(u => u.Contracts.AddAsync(It.IsAny<ContractEntity>(), It.IsAny<CancellationToken>()))
                .Callback<ContractEntity, CancellationToken>((c, _) => savedContract = c)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var expectedDto = new ContractDetailDto { Id = Guid.NewGuid(), Status = "Active" };
            _mockMapper.Setup(m => m.Map<ContractDetailDto>(It.IsAny<ContractEntity>()))
                .Returns(expectedDto);

            // Act
            var result = await _contractService.CreateContractAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(savedContract);
            Assert.Equal(request.RoomId, savedContract.RoomId);
            Assert.Equal(request.DepositAmount, savedContract.DepositAmount.Amount);
            Assert.Equal("Active", savedContract.Status.Value);
            
            // Check that room status was updated
            Assert.Equal("Occupied", roomEntity.Status.Value);
            _mockUnitOfWork.Verify(u => u.Rooms.Update(roomEntity), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TerminateContractAsync_ShouldUpdateContractAndRoomStatus()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var request = new TerminateContractRequest
            {
                Reason = "Moved out",
                NewRoomStatus = "Maintenance"
            };

            var contractEntity = new ContractEntity
            {
                Id = contractId,
                RoomId = roomId,
                Status = new Status("Active")
            };

            var roomEntity = new RoomEntity
            {
                Id = roomId,
                Status = new Status("Occupied")
            };

            _mockUnitOfWork.Setup(u => u.Contracts.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contractEntity);
            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);

            var expectedDto = new ContractDetailDto { Id = contractId, Status = "Terminated" };
            _mockMapper.Setup(m => m.Map<ContractDetailDto>(It.IsAny<ContractEntity>()))
                .Returns(expectedDto);

            // Act
            var result = await _contractService.TerminateContractAsync(contractId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Terminated", contractEntity.Status.Value);
            Assert.Equal("Maintenance", roomEntity.Status.Value);
            _mockUnitOfWork.Verify(u => u.Contracts.Update(contractEntity), Times.Once);
            _mockUnitOfWork.Verify(u => u.Rooms.Update(roomEntity), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task SendRenewalNotificationAsync_ShouldPublishEvent_WhenDataIsValid()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var contractEntity = new ContractEntity
            {
                Id = contractId,
                RoomId = Guid.NewGuid(),
                PropertyId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid(),
                Status = new Status("Active"),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
            };

            var roomEntity = new RoomEntity { Id = contractEntity.RoomId, RoomNumber = "101" };
            var propertyEntity = new PropertyEntity { Id = contractEntity.PropertyId, PropertyName = "Building A" };
            var residentEntity = new ResidentEntity { Id = contractEntity.ResidentId, FullName = "John Doe", Email = "john@example.com" };

            _mockUnitOfWork.Setup(u => u.Contracts.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contractEntity);
            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(contractEntity.RoomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);
            _mockUnitOfWork.Setup(u => u.Properties.GetByIdAsync(contractEntity.PropertyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(propertyEntity);
            _mockUnitOfWork.Setup(u => u.Residents.GetByIdAsync(contractEntity.ResidentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(residentEntity);

            // Act
            await _contractService.SendRenewalNotificationAsync(contractId);

            // Assert
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<NotificationContractRenewEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
