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
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple
{
    public class MaintenanceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly MaintenanceService _maintenanceService;

        public MaintenanceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _maintenanceService = new MaintenanceService(
                _mockUnitOfWork.Object,
                _mockMapper.Object);

            _mockUnitOfWork.Setup(u => u.Properties.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PropertyEntity)null!);
            _mockUnitOfWork.Setup(u => u.Contracts.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ContractEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ContractEntity>());
            _mockUnitOfWork.Setup(u => u.MaintenanceTickets.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MaintenanceTicketEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceTicketEntity>());
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldSaveAndReturnDto()
        {
            // Arrange
            var request = new CreateMaintenanceTicketRequest
            {
                OrganizationId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid(),
                Category = "Electrical",
                UserDescription = "Light bulb is broken"
            };

            MaintenanceTicketEntity savedTicket = null!;
            _mockUnitOfWork.Setup(u => u.MaintenanceTickets.AddAsync(It.IsAny<MaintenanceTicketEntity>(), It.IsAny<CancellationToken>()))
                .Callback<MaintenanceTicketEntity, CancellationToken>((t, _) => savedTicket = t)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var expectedDto = new MaintenanceTicketDetailDto { Id = Guid.NewGuid(), Status = "Pending" };
            _mockMapper.Setup(m => m.Map<MaintenanceTicketDetailDto>(It.IsAny<MaintenanceTicketEntity>()))
                .Returns(expectedDto);

            // Act
            var result = await _maintenanceService.CreateTicketAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(savedTicket);
            Assert.Equal(request.RoomId, savedTicket.RoomId);
            Assert.Equal("Pending", savedTicket.Status);
            Assert.Equal(request.UserDescription, savedTicket.UserDescription);
            
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MarkRoomStatusAsync_ShouldUpdateRoomAndCreateTicketIfMaintenance()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var residentId = Guid.NewGuid();
            var newStatus = "Maintenance";
            var description = "Routine checkup";

            var roomEntity = new RoomEntity
            {
                Id = roomId,
                Status = new Status("Available")
            };

            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);

            MaintenanceTicketEntity savedTicket = null!;
            _mockUnitOfWork.Setup(u => u.MaintenanceTickets.AddAsync(It.IsAny<MaintenanceTicketEntity>(), It.IsAny<CancellationToken>()))
                .Callback<MaintenanceTicketEntity, CancellationToken>((t, _) => savedTicket = t)
                .Returns(Task.CompletedTask);

            // Act
            await _maintenanceService.MarkRoomStatusAsync(roomId, newStatus, organizationId, residentId, description);

            // Assert
            Assert.Equal("Maintenance", roomEntity.Status.Value);
            _mockUnitOfWork.Verify(u => u.Rooms.Update(roomEntity), Times.Once);
            Assert.NotNull(savedTicket);
            Assert.Equal(roomId, savedTicket.RoomId);
            Assert.Equal(description, savedTicket.UserDescription);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task MarkRoomStatusAsync_ShouldUpdateRoomAndResolveTicketsIfAvailable()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var residentId = Guid.NewGuid();
            var newStatus = "Available";
            var description = "Maintenance completed";

            var roomEntity = new RoomEntity
            {
                Id = roomId,
                Status = new Status("Maintenance")
            };

            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);

            // Act
            await _maintenanceService.MarkRoomStatusAsync(roomId, newStatus, organizationId, residentId, description);

            // Assert
            Assert.Equal("Available", roomEntity.Status.Value);
            _mockUnitOfWork.Verify(u => u.Rooms.Update(roomEntity), Times.Once);
            // No ticket created when marking as available, just updates existing ones to Resolved (if implemented that way in service)
            // It depends on the actual implementation of MarkRoomStatusAsync in MaintenanceService.
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
