using System;
using System.Collections.Generic;
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
    public class ResidentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ResidentService _residentService;

        public ResidentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _residentService = new ResidentService(
                _mockUnitOfWork.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task CreateResidentAccountAsync_ShouldSaveResidentAndAccount()
        {
            // Arrange
            var request = new CreateResidentAccountRequest
            {
                Name = "Nguyen Van A",
                Sdt = "0987654321",
                IdentityCardNumber = "001234567890",
                Sex = "Male",
                Address = "Hanoi"
            };

            _mockUnitOfWork.Setup(u => u.Accounts.GetByPhoneAsync(request.Sdt, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountEntity?)null);

            ResidentEntity savedResident = null!;
            _mockUnitOfWork.Setup(u => u.Residents.AddAsync(It.IsAny<ResidentEntity>(), It.IsAny<CancellationToken>()))
                .Callback<ResidentEntity, CancellationToken>((r, _) => savedResident = r)
                .Returns(Task.CompletedTask);

            AccountEntity savedAccount = null!;
            _mockUnitOfWork.Setup(u => u.Accounts.AddAsync(It.IsAny<AccountEntity>(), It.IsAny<CancellationToken>()))
                .Callback<AccountEntity, CancellationToken>((a, _) => savedAccount = a)
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _residentService.CreateResidentAccountAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(savedResident);
            Assert.NotNull(savedAccount);
            
            Assert.Equal(request.Name, savedResident.FullName.Value);
            Assert.Equal(request.Sdt, savedResident.Phone.Value);
            Assert.Equal(request.Sdt, savedAccount.Phone); // Account username is usually the phone number
            Assert.Equal(savedAccount.Id, savedResident.AccountId); // Link between account and resident
            
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateResidentAccountAsync_ThrowsException_IfPhoneExists()
        {
            // Arrange
            var request = new CreateResidentAccountRequest
            {
                Name = "Nguyen Van A",
                Sdt = "0987654321"
            };

            _mockUnitOfWork.Setup(u => u.Accounts.GetByPhoneAsync(request.Sdt, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountEntity());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _residentService.CreateResidentAccountAsync(request));
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
