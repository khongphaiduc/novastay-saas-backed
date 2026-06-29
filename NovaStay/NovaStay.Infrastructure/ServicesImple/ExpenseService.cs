using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public Task<IReadOnlyList<ExpenseDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        return _expenseRepository.GetByPropertyAsync(
            organizationId,
            propertyId,
            status,
            paymentMethod,
            cancellationToken);
    }

    public Task<ExpenseDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ExpenseNumber))
        {
            throw new ArgumentException("ExpenseNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PayeeName))
        {
            throw new ArgumentException("PayeeName is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0.");
        }

        request.ExpenseNumber = request.ExpenseNumber.Trim();
        request.PayeeName = request.PayeeName.Trim();
        request.ReferenceCode = string.IsNullOrWhiteSpace(request.ReferenceCode) ? null : request.ReferenceCode.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        return _expenseRepository.CreateAsync(
            organizationId,
            propertyId,
            request,
            cancellationToken);
    }
}
