using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;
using NovaStay.Infrastructure.ServicesImple;
using Xunit;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple
{
    public class InputValidationHardeningTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IRoomRepository> _rooms = new();
        private readonly Mock<IAssetRepository> _assets = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IMinioStorageService> _storage = new();
        private readonly Mock<IPublishEndpoint> _publish = new();

        public InputValidationHardeningTests()
        {
            _unitOfWork.Setup(u => u.Rooms).Returns(_rooms.Object);
            _unitOfWork.Setup(u => u.Assets).Returns(_assets.Object);
        }

        private RoomService CreateRoomService() => new(_unitOfWork.Object, _mapper.Object, _storage.Object);
        private AssetService CreateAssetService() => new(_unitOfWork.Object, _mapper.Object);
        private ContractService CreateContractService() => new(_unitOfWork.Object, _mapper.Object, _publish.Object);

        private static CreateRoomRequest ValidRoom(Action<CreateRoomRequest>? change = null)
        {
            var request = new CreateRoomRequest
            {
                PropertyId = Guid.NewGuid(),
                RoomNumber = "A101",
                Floor = 1,
                BasePrice = 3_000_000,
                MaxOccupants = 2
            };
            change?.Invoke(request);
            return request;
        }

        [Fact]
        public async Task CreateRoom_WithDuplicateNumberInSameProperty_Throws()
        {
            _rooms.Setup(r => r.RoomNumberExistsAsync(It.IsAny<Guid>(), "A101", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateRoomService().CreateRoomAsync(ValidRoom()));
            _rooms.Verify(r => r.AddAsync(It.IsAny<RoomEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateRoom_WithBlankNumber_Throws(string roomNumber)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().CreateRoomAsync(ValidRoom(r => r.RoomNumber = roomNumber)));
        }

        [Fact]
        public async Task CreateRoom_WithNumberLongerThan20_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().CreateRoomAsync(ValidRoom(r => r.RoomNumber = new string('9', 21))));
        }

        [Fact]
        public async Task CreateRoom_WithNegativeBasePrice_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().CreateRoomAsync(ValidRoom(r => r.BasePrice = -1)));
        }

        [Fact]
        public async Task CreateRoom_WithZeroMaxOccupants_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().CreateRoomAsync(ValidRoom(r => r.MaxOccupants = 0)));
        }

        [Fact]
        public async Task UpdateMaxOccupants_BelowOne_Throws()
        {
            var roomId = Guid.NewGuid();
            _rooms.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoomEntity { Id = roomId, PropertyId = Guid.NewGuid(), RoomNumber = "A101" });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().UpdateMaxOccupantsAsync(roomId, new UpdateMaxOccupantsRequest { MaxOccupants = 0 }));
        }

        [Fact]
        public async Task UpdateBasePrice_Negative_Throws()
        {
            var roomId = Guid.NewGuid();
            _rooms.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoomEntity { Id = roomId, PropertyId = Guid.NewGuid(), RoomNumber = "A101" });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateRoomService().UpdateBasePriceAsync(roomId, new UpdateBasePriceRequest { BasePrice = -5 }));
        }

        [Fact]
        public async Task CreateAsset_WithNameLongerThan100_Throws()
        {
            var request = new CreateAssetRequest { OrganizationId = Guid.NewGuid(), AssetName = new string('a', 101) };
            await Assert.ThrowsAsync<ArgumentException>(() => CreateAssetService().CreateAssetAsync(request));
        }

        [Fact]
        public async Task CreateAsset_WithBrandLongerThan50_Throws()
        {
            var request = new CreateAssetRequest { OrganizationId = Guid.NewGuid(), AssetName = "Tủ lạnh", Brand = new string('b', 51) };
            await Assert.ThrowsAsync<ArgumentException>(() => CreateAssetService().CreateAssetAsync(request));
        }

        [Fact]
        public async Task CreateAsset_WithNegativeBaseValue_Throws()
        {
            var request = new CreateAssetRequest { OrganizationId = Guid.NewGuid(), AssetName = "Tủ lạnh", BaseValue = -100 };
            await Assert.ThrowsAsync<ArgumentException>(() => CreateAssetService().CreateAssetAsync(request));
        }

        [Fact]
        public async Task UpdateAsset_WithBlankName_Throws()
        {
            var assetId = Guid.NewGuid();
            _assets.Setup(a => a.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetEntity { Id = assetId, OrganizationId = Guid.NewGuid(), AssetName = "Tủ" });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateAssetService().UpdateAssetAsync(assetId, new UpdateAssetRequest { AssetName = "   " }));
        }

        [Fact]
        public async Task CreateContract_WithNegativeDeposit_Throws()
        {
            var request = new CreateContractRequest
            {
                OrganizationId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid(),
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                DepositAmount = -1
            };
            await Assert.ThrowsAsync<ArgumentException>(() => CreateContractService().CreateContractAsync(request));
        }

        [Fact]
        public async Task CreateContract_WithNegativeBrokerCommission_Throws()
        {
            var request = new CreateContractRequest
            {
                OrganizationId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid(),
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                DepositAmount = 1_000_000,
                BrokerCommission = -10
            };
            await Assert.ThrowsAsync<ArgumentException>(() => CreateContractService().CreateContractAsync(request));
        }

        [Fact]
        public async Task UpdateContract_WithNegativeDeposit_Throws()
        {
            var contractId = Guid.NewGuid();
            var contracts = new Mock<IContractRepository>();
            contracts.Setup(c => c.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContractEntity
                {
                    Id = contractId,
                    StartDate = new DateOnly(2026, 1, 1),
                    EndDate = new DateOnly(2026, 12, 31)
                });
            _unitOfWork.Setup(u => u.Contracts).Returns(contracts.Object);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateContractService().UpdateContractAsync(contractId, new UpdateContractRequest { DepositAmount = -1 }));
        }
    }
}
