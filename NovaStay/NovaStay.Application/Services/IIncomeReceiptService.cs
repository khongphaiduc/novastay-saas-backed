using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.Application.Services;

public interface IIncomeReceiptService
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

    Task<IncomeReceiptDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default);

    Task<IncomeReceiptDto> UpdateStatusAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeReceiptId,
        UpdateApprovalStatusRequest request,
        CancellationToken cancellationToken = default);
}
