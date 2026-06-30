using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.Application.Common.Interfaces;

public interface IIncomeReceiptRepository
{
    Task<IReadOnlyList<IncomeReceiptDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default);

    Task<IncomeReceiptDto?> GetByIdAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeReceiptId,
        CancellationToken cancellationToken = default);

    Task<bool> PropertyExistsAsync(
        Guid organizationId,
        Guid propertyId,
        CancellationToken cancellationToken = default);

    Task<bool> ReceiptNumberExistsAsync(
        Guid organizationId,
        string receiptNumber,
        CancellationToken cancellationToken = default);

    Task<bool> RoomExistsAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<bool> ResidentExistsAsync(
        Guid residentId,
        CancellationToken cancellationToken = default);

    Task<bool> StaffUserExistsAsync(
        Guid organizationId,
        Guid staffUserId,
        CancellationToken cancellationToken = default);

    Task<Guid> EnsureIncomeCategoryAsync(
        Guid organizationId,
        Guid? incomeCategoryId,
        NovaStay.Domain.Enums.IncomeCategory incomeType,
        CancellationToken cancellationToken = default);

    Task<IncomeReceiptDto> AddAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeCategoryId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default);

    Task<IncomeReceiptDto> UpdateStatusAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeReceiptId,
        NovaStay.Domain.Enums.ApprovalStatus status,
        CancellationToken cancellationToken = default);
}
