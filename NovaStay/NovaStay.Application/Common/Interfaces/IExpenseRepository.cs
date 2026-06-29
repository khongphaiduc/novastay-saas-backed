using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.Application.Common.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<ExpenseDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default);

    Task<ExpenseDto?> GetByIdAsync(
        Guid organizationId,
        Guid propertyId,
        Guid expenseId,
        CancellationToken cancellationToken = default);

    Task<ExpenseDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default);
}
