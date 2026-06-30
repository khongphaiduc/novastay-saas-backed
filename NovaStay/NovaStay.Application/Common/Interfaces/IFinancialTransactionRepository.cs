using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;

namespace NovaStay.Application.Common.Interfaces;

public interface IFinancialTransactionRepository
{
    Task<PagedResult<FinancialTransactionDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        string? transactionType = null,
        string? search = null,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
