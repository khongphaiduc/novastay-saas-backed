using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class FinancialTransactionService : IFinancialTransactionService
{
    private readonly IFinancialTransactionRepository _financialTransactionRepository;

    public FinancialTransactionService(IFinancialTransactionRepository financialTransactionRepository)
    {
        _financialTransactionRepository = financialTransactionRepository;
    }

    public Task<PagedResult<FinancialTransactionDto>?> GetByPropertyAsync(
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
        CancellationToken cancellationToken = default)
    {
        if (pageIndex <= 0)
        {
            throw new ArgumentException("pageIndex must be greater than 0.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException("pageSize must be greater than 0.");
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            var normalizedType = transactionType.Trim().ToLowerInvariant();
            if (normalizedType is not ("expense" or "income" or "incomereceipt"))
            {
                throw new ArgumentException("transactionType must be 'expense' or 'income'.");
            }

            transactionType = normalizedType;
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            throw new ArgumentException("fromDate must be less than or equal to toDate.");
        }

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        return _financialTransactionRepository.GetByPropertyAsync(
            organizationId,
            propertyId,
            transactionType,
            search,
            status,
            paymentMethod,
            fromDate?.Date,
            toDate?.Date,
            pageIndex,
            pageSize,
            cancellationToken);
    }
}
