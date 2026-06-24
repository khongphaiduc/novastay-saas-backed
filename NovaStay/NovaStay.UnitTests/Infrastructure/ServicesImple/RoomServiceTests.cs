using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple
{
    public class RoomServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMinioStorageService> _mockStorage;
        private readonly RoomService _roomService;

        public RoomServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockStorage = new Mock<IMinioStorageService>();

            _roomService = new RoomService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockStorage.Object);
        }

        [Fact]
        public async Task CreateRoomAsync_ShouldGenerateValidGuidAndSave()
        {
            // Arrange
            var propertyId = Guid.NewGuid();
            var request = new CreateRoomRequest
            {
                PropertyId = propertyId,
                RoomNumber = "101",
                Floor = 1,
                BasePrice = 3500000,
                Status = "Available",
                MaxOccupants = 2,
                AmenitiesJson = "[\"wifi\",\"ac\"]"
            };

            RoomEntity savedRoom = null!;

            _mockUnitOfWork.Setup(u => u.Rooms.AddAsync(It.IsAny<RoomEntity>(), It.IsAny<CancellationToken>()))
                .Callback<RoomEntity, CancellationToken>((r, c) => savedRoom = r)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockUnitOfWork.Setup(u => u.Rooms.GetRoomWithImagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoomDto)null!); // to fallback to mapper

            var expectedDto = new RoomDto { Id = Guid.NewGuid(), RoomNumber = "101" };
            _mockMapper.Setup(m => m.Map<RoomDto>(It.IsAny<RoomEntity>()))
                .Returns(expectedDto);

            // Act
            var result = await _roomService.CreateRoomAsync(request);

            // Assert
            Assert.NotNull(savedRoom);
            Assert.NotEqual(Guid.Empty, savedRoom.Id); // Verify the fix!
            Assert.Equal(request.PropertyId, savedRoom.PropertyId);
            Assert.Equal(request.RoomNumber, savedRoom.RoomNumber.Value);
            
            _mockUnitOfWork.Verify(u => u.Rooms.AddAsync(It.IsAny<RoomEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadRoomImageAsync_ShouldGenerateValidGuidAndSave()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var request = new UploadRoomImageRequest
            {
                FileName = "test.png",
                ContentType = "image/png",
                ImageStream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 }),
                FileSize = 3,
                IsCover = true
            };

            var roomEntity = new RoomEntity { Id = roomId };
            
            _mockUnitOfWork.Setup(u => u.Rooms.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomEntity);

            _mockStorage.Setup(s => s.UploadImageAsync(It.IsAny<System.IO.Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://minio.local/test.png");

            _mockUnitOfWork.Setup(u => u.RoomImages.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RoomImageEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new System.Collections.Generic.List<RoomImageEntity>());

            RoomImageEntity savedImage = null!;
            _mockUnitOfWork.Setup(u => u.RoomImages.AddAsync(It.IsAny<RoomImageEntity>(), It.IsAny<CancellationToken>()))
                .Callback<RoomImageEntity, CancellationToken>((r, c) => savedImage = r)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<RoomImageDto>(It.IsAny<RoomImageEntity>()))
                .Returns(new RoomImageDto());

            // Act
            var result = await _roomService.UploadRoomImageAsync(roomId, request);

            // Assert
            Assert.NotNull(savedImage);
            Assert.NotEqual(Guid.Empty, savedImage.Id); // Verify the fix!
            Assert.Equal(roomId, savedImage.RoomId);
            Assert.Equal("https://minio.local/test.png", savedImage.ImageUrl);

            _mockUnitOfWork.Verify(u => u.RoomImages.AddAsync(It.IsAny<RoomImageEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRoomImageAsync_ShouldRemoveImageAndSave()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var imageId = Guid.NewGuid();
            var existingImage = new RoomImageEntity { Id = imageId, RoomId = roomId };

            _mockUnitOfWork.Setup(u => u.RoomImages.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<RoomImageEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new System.Collections.Generic.List<RoomImageEntity> { existingImage });

            _mockUnitOfWork.Setup(u => u.RoomImages.Remove(It.IsAny<RoomImageEntity>()));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _roomService.DeleteRoomImageAsync(roomId, imageId);

            // Assert
            _mockUnitOfWork.Verify(u => u.RoomImages.Remove(existingImage), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
