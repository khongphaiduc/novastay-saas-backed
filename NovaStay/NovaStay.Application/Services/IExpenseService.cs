using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.Application.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default);

    Task<ExpenseDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default);
}
