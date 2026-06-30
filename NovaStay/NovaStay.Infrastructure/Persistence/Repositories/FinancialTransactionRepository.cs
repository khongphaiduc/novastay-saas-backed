using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal sealed class FinancialTransactionRepository : IFinancialTransactionRepository
{
    private readonly HostContext _context;

    public FinancialTransactionRepository(HostContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FinancialTransactionDto>?> GetByPropertyAsync(
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
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == propertyId && entity.OrganizationId == organizationId,
                cancellationToken);

        if (!propertyExists)
        {
            return null;
        }

        var includeExpenses = transactionType is null or "" or "expense";
        var includeIncomeReceipts = transactionType is null or "" or "income" or "incomereceipt";
        var statusValue = status?.ToString();
        var paymentMethodValue = paymentMethod?.ToString();
        var normalizedSearch = search?.ToLower();

        var expenseQuery = _context.Expenses
            .AsNoTracking()
            .Where(expense =>
                includeExpenses &&
                expense.OrganizationId == organizationId &&
                expense.PropertyId == propertyId);

        if (statusValue is not null)
        {
            expenseQuery = expenseQuery.Where(expense => expense.Status == statusValue);
        }

        if (paymentMethodValue is not null)
        {
            expenseQuery = expenseQuery.Where(expense => expense.PaymentMethod == paymentMethodValue);
        }

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            expenseQuery = expenseQuery.Where(expense =>
                expense.ExpenseNumber.ToLower().Contains(normalizedSearch) ||
                expense.PayeeName.ToLower().Contains(normalizedSearch) ||
                (expense.ReferenceCode != null && expense.ReferenceCode.ToLower().Contains(normalizedSearch)));
        }

        if (fromDate.HasValue)
        {
            expenseQuery = expenseQuery.Where(expense => expense.SpentAt.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            expenseQuery = expenseQuery.Where(expense => expense.SpentAt.Date <= toDate.Value);
        }

        var projectedExpenses = expenseQuery.Select(expense => new FinancialTransactionDto
        {
            Id = expense.Id,
            OrganizationId = expense.OrganizationId,
            PropertyId = expense.PropertyId ?? Guid.Empty,
            TransactionType = "Expense",
            CategoryId = expense.ExpenseCategoryId,
            CategoryCode = expense.ExpenseCategory.CategoryCode,
            CategoryName = expense.ExpenseCategory.CategoryName,
            RoomId = expense.RoomId,
            RoomNumber = expense.Room != null ? expense.Room.RoomNumber : null,
            ResidentId = null,
            ResidentName = null,
            StaffUserId = expense.ApprovedByStaffUserId ?? expense.CreatedByStaffUserId,
            StaffUserName = expense.ApprovedByStaffUser != null
                ? expense.ApprovedByStaffUser.FullName
                : expense.CreatedByStaffUser != null
                    ? expense.CreatedByStaffUser.FullName
                    : null,
            ReferenceNumber = expense.ExpenseNumber,
            TypeRaw = expense.ExpenseType,
            CounterpartyName = expense.PayeeName,
            Amount = expense.Amount,
            TransactionDate = expense.SpentAt,
            PaymentMethod = null,
            PaymentMethodRaw = expense.PaymentMethod,
            Status = null,
            StatusRaw = expense.Status,
            ReferenceCode = expense.ReferenceCode,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        });

        var incomeReceiptQuery = _context.IncomeReceipts
            .AsNoTracking()
            .Where(receipt =>
                includeIncomeReceipts &&
                receipt.OrganizationId == organizationId &&
                receipt.PropertyId == propertyId);

        if (statusValue is not null)
        {
            incomeReceiptQuery = incomeReceiptQuery.Where(receipt => receipt.Status == statusValue);
        }

        if (paymentMethodValue is not null)
        {
            incomeReceiptQuery = incomeReceiptQuery.Where(receipt => receipt.PaymentMethod == paymentMethodValue);
        }

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            incomeReceiptQuery = incomeReceiptQuery.Where(receipt =>
                receipt.ReceiptNumber.ToLower().Contains(normalizedSearch) ||
                receipt.PayerName.ToLower().Contains(normalizedSearch) ||
                (receipt.ReferenceCode != null && receipt.ReferenceCode.ToLower().Contains(normalizedSearch)));
        }

        if (fromDate.HasValue)
        {
            incomeReceiptQuery = incomeReceiptQuery.Where(receipt => receipt.CollectedAt.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            incomeReceiptQuery = incomeReceiptQuery.Where(receipt => receipt.CollectedAt.Date <= toDate.Value);
        }

        var projectedIncomeReceipts = incomeReceiptQuery.Select(receipt => new FinancialTransactionDto
        {
            Id = receipt.Id,
            OrganizationId = receipt.OrganizationId,
            PropertyId = receipt.PropertyId,
            TransactionType = "IncomeReceipt",
            CategoryId = receipt.IncomeCategoryId,
            CategoryCode = receipt.IncomeCategory.CategoryCode,
            CategoryName = receipt.IncomeCategory.CategoryName,
            RoomId = receipt.RoomId,
            RoomNumber = receipt.Room != null ? receipt.Room.RoomNumber : null,
            ResidentId = receipt.ResidentId,
            ResidentName = receipt.Resident != null ? receipt.Resident.FullName : null,
            StaffUserId = receipt.CollectedByStaffUserId,
            StaffUserName = receipt.CollectedByStaffUser != null ? receipt.CollectedByStaffUser.FullName : null,
            ReferenceNumber = receipt.ReceiptNumber,
            TypeRaw = receipt.IncomeType,
            CounterpartyName = receipt.PayerName,
            Amount = receipt.Amount,
            TransactionDate = receipt.CollectedAt,
            PaymentMethod = null,
            PaymentMethodRaw = receipt.PaymentMethod,
            Status = null,
            StatusRaw = receipt.Status,
            ReferenceCode = receipt.ReferenceCode,
            Description = receipt.Description,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt
        });

        var combinedQuery = projectedExpenses.Concat(projectedIncomeReceipts);  // bind the two queries together
        var totalCount = await combinedQuery.CountAsync(cancellationToken);

        var items = await combinedQuery
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.PaymentMethod = TryParseEnum<PaymentMethod>(item.PaymentMethodRaw);
            item.Status = TryParseEnum<ApprovalStatus>(item.StatusRaw);
        }

        return new PagedResult<FinancialTransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    private static TEnum? TryParseEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsedValue)
            ? parsedValue
            : null;
    }
}
