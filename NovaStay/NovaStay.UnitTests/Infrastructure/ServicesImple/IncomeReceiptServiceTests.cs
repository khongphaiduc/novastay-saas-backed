using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public sealed class IncomeReceiptServiceTests
{
    [Fact]
    public async Task GetByPropertyAsync_ShouldDelegateToRepository()
    {
        IReadOnlyList<IncomeReceiptDto> expected =
        [
            new IncomeReceiptDto
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = "PT001"
            }
        ];

        var repository = new Mock<IIncomeReceiptRepository>();
        repository
            .Setup(instance => instance.GetByPropertyAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ApprovalStatus?>(),
                It.IsAny<PaymentMethod?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new IncomeReceiptService(repository.Object);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var result = await service.GetByPropertyAsync(
            organizationId,
            propertyId,
            ApprovalStatus.Pending,
            PaymentMethod.Cash);

        Assert.Same(expected, result);
        repository.Verify(instance => instance.GetByPropertyAsync(
            organizationId,
            propertyId,
            ApprovalStatus.Pending,
            PaymentMethod.Cash,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldEnsureCategoryAndCreateReceipt()
    {
        var response = new IncomeReceiptDto
        {
            Id = Guid.NewGuid(),
            ReceiptNumber = "PT001",
            IncomeCategoryCode = "SERVICE_FEE"
        };

        var repository = new Mock<IIncomeReceiptRepository>();
        repository
            .Setup(instance => instance.PropertyExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository
            .Setup(instance => instance.ReceiptNumberExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(instance => instance.RoomExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository
            .Setup(instance => instance.ResidentExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository
            .Setup(instance => instance.StaffUserExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var incomeCategoryId = Guid.NewGuid();
        repository
            .Setup(instance => instance.EnsureIncomeCategoryAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<IncomeCategory>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(incomeCategoryId);

        repository
            .Setup(instance => instance.AddAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CreateIncomeReceiptRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new IncomeReceiptService(repository.Object);
        var organizationId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var residentId = Guid.NewGuid();
        var staffUserId = Guid.NewGuid();
        var request = new CreateIncomeReceiptRequest
        {
            IncomeType = IncomeCategory.ServiceFee,
            RoomId = roomId,
            ResidentId = residentId,
            CollectedByStaffUserId = staffUserId,
            ReceiptNumber = "  PT001  ",
            PayerName = "  Nguyen Van A  ",
            Amount = 500000,
            CollectedAt = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.BankTransfer,
            ReferenceCode = "  REF001  ",
            Description = "  Thu phi dich vu  "
        };

        var result = await service.CreateAsync(organizationId, propertyId, request);

        Assert.Same(response, result);
        Assert.Equal("PT001", request.ReceiptNumber);
        Assert.Equal("Nguyen Van A", request.PayerName);
        Assert.Equal("REF001", request.ReferenceCode);
        Assert.Equal("Thu phi dich vu", request.Description);

        repository.Verify(instance => instance.EnsureIncomeCategoryAsync(
            organizationId,
            request.IncomeCategoryId,
            IncomeCategory.ServiceFee,
            It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(instance => instance.AddAsync(
            organizationId,
            propertyId,
            incomeCategoryId,
            request,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenReceiptNumberAlreadyExists()
    {
        var repository = new Mock<IIncomeReceiptRepository>();
        repository
            .Setup(instance => instance.PropertyExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository
            .Setup(instance => instance.ReceiptNumberExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new IncomeReceiptService(repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreateIncomeReceiptRequest
            {
                ReceiptNumber = "PT001",
                PayerName = "Nguyen Van A",
                Amount = 1000,
                CollectedAt = DateTime.UtcNow,
                IncomeType = IncomeCategory.Other,
                PaymentMethod = PaymentMethod.Cash
            }));

        repository.Verify(instance => instance.EnsureIncomeCategoryAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid?>(),
            It.IsAny<IncomeCategory>(),
            It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(instance => instance.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CreateIncomeReceiptRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
