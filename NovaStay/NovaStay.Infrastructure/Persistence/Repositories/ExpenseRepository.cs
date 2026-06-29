using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ContextDB;
using System.Text.RegularExpressions;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal sealed class ExpenseRepository : IExpenseRepository
{
    private readonly HostContext _context;

    public ExpenseRepository(HostContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExpenseDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
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

        var query = _context.Expenses
            .AsNoTracking()
            .Include(expense => expense.ExpenseCategory)
            .Include(expense => expense.Room)
            .Include(expense => expense.ApprovedByStaffUser)
            .Include(expense => expense.CreatedByStaffUser)
            .Where(expense => expense.OrganizationId == organizationId && expense.PropertyId == propertyId);

        if (status.HasValue)
        {
            var statusValue = status.Value.ToString();
            query = query.Where(expense => expense.Status == statusValue);
        }

        if (paymentMethod.HasValue)
        {
            var paymentMethodValue = paymentMethod.Value.ToString();
            query = query.Where(expense => expense.PaymentMethod == paymentMethodValue);
        }

        var expenses = await query
            .OrderByDescending(expense => expense.SpentAt)
            .ThenByDescending(expense => expense.CreatedAt)
            .ToListAsync(cancellationToken);

        return expenses.Select(MapToDto).ToList();
    }

    public async Task<ExpenseDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == propertyId && entity.OrganizationId == organizationId,
                cancellationToken);

        if (!propertyExists)
        {
            throw new KeyNotFoundException("Property not found in organization.");
        }

        var category = await ResolveExpenseCategoryAsync(
            organizationId,
            request,
            cancellationToken);


        // check if the expense number already exists in the organization
        var duplicatedExpenseNumber = await _context.Expenses
            .AsNoTracking()
            .AnyAsync(
                entity => entity.OrganizationId == organizationId && entity.ExpenseNumber == request.ExpenseNumber,
                cancellationToken);

        if (duplicatedExpenseNumber)
        {
            throw new InvalidOperationException("Expense number already exists in organization.");
        }

        if (request.RoomId.HasValue)   // Check whether a nullable variable has a value or not.
        {
            var roomExists = await _context.Rooms
                .AsNoTracking()
                .AnyAsync(
                    entity => entity.Id == request.RoomId.Value && entity.PropertyId == propertyId,
                    cancellationToken);

            if (!roomExists)
            {
                throw new KeyNotFoundException("Room not found in property.");
            }
        }

        if (request.RelatedMaintenanceTicketId.HasValue)
        {
            var maintenanceTicketExists = await _context.MaintenanceTickets
                .AsNoTracking()
                .AnyAsync(
                    entity => entity.Id == request.RelatedMaintenanceTicketId.Value &&
                              entity.OrganizationId == organizationId &&
                              entity.Room.PropertyId == propertyId,
                    cancellationToken);

            if (!maintenanceTicketExists)
            {
                throw new KeyNotFoundException("Maintenance ticket not found in property.");
            }
        }

        if (request.RelatedBrokerId.HasValue)
        {
            var brokerExists = await _context.Brokers
                .AsNoTracking()
                .AnyAsync(
                    entity => entity.Id == request.RelatedBrokerId.Value && entity.OrganizationId == organizationId,
                    cancellationToken);

            if (!brokerExists)
            {
                throw new KeyNotFoundException("Broker not found in organization.");
            }
        }

        if (request.ApprovedByStaffUserId.HasValue)
        {
            var approvedByExists = await _context.StaffUsers
                .AsNoTracking()
                .AnyAsync(
                    entity => entity.Id == request.ApprovedByStaffUserId.Value && entity.OrganizationId == organizationId,
                    cancellationToken);

            if (!approvedByExists)
            {
                throw new KeyNotFoundException("ApprovedByStaffUser not found in organization.");
            }
        }

        if (request.CreatedByStaffUserId.HasValue)
        {
            var createdByExists = await _context.StaffUsers
                .AsNoTracking()
                .AnyAsync(
                    entity => entity.Id == request.CreatedByStaffUserId.Value && entity.OrganizationId == organizationId,
                    cancellationToken);

            if (!createdByExists)
            {
                throw new KeyNotFoundException("CreatedByStaffUser not found in organization.");
            }
        }

        var expense = new Models.Expense
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            ExpenseCategoryId = category.Id,
            RoomId = request.RoomId,
            RelatedMaintenanceTicketId = request.RelatedMaintenanceTicketId,
            RelatedBrokerId = request.RelatedBrokerId,
            ApprovedByStaffUserId = request.ApprovedByStaffUserId,
            CreatedByStaffUserId = request.CreatedByStaffUserId,
            ExpenseNumber = request.ExpenseNumber,
            ExpenseType = request.ExpenseType.ToString(),
            PayeeName = request.PayeeName,
            Amount = request.Amount,
            SpentAt = request.SpentAt,
            PaymentMethod = request.PaymentMethod.ToString(),
            Status = request.Status.ToString(),
            ReferenceCode = request.ReferenceCode,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Expenses.AddAsync(expense, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var createdExpense = await _context.Expenses
            .AsNoTracking()
            .Include(entity => entity.ExpenseCategory)
            .Include(entity => entity.Room)
            .Include(entity => entity.ApprovedByStaffUser)
            .Include(entity => entity.CreatedByStaffUser)
            .FirstAsync(entity => entity.Id == expense.Id, cancellationToken);

        return MapToDto(createdExpense);
    }

    private async Task<Models.ExpenseCategory> ResolveExpenseCategoryAsync(
        Guid organizationId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ExpenseCategoryId.HasValue)
        {
            var existingCategory = await _context.ExpenseCategories
                .FirstOrDefaultAsync(
                    entity => entity.Id == request.ExpenseCategoryId.Value && entity.OrganizationId == organizationId,
                    cancellationToken);

            if (existingCategory is not null)
            {
                return existingCategory;
            }
        }

        var categoryCode = BuildCategoryCode(request.ExpenseType);
        var categoryName = BuildCategoryName(request.ExpenseType);

        var category = await _context.ExpenseCategories
            .FirstOrDefaultAsync(
                entity => entity.OrganizationId == organizationId && entity.CategoryCode == categoryCode,
                cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = new Models.ExpenseCategory
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ExpenseCategories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    private static ExpenseDto MapToDto(Models.Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            OrganizationId = expense.OrganizationId,
            PropertyId = expense.PropertyId ?? Guid.Empty,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            ExpenseCategoryCode = expense.ExpenseCategory.CategoryCode,
            ExpenseCategoryName = expense.ExpenseCategory.CategoryName,
            RoomId = expense.RoomId,
            RoomNumber = expense.Room?.RoomNumber,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseType = TryParseEnum<ExpenseCategory>(expense.ExpenseType),
            ExpenseTypeRaw = expense.ExpenseType,
            PayeeName = expense.PayeeName,
            Amount = expense.Amount,
            SpentAt = expense.SpentAt,
            PaymentMethod = TryParseEnum<PaymentMethod>(expense.PaymentMethod),
            PaymentMethodRaw = expense.PaymentMethod,
            Status = TryParseEnum<ApprovalStatus>(expense.Status),
            StatusRaw = expense.Status,
            ReferenceCode = expense.ReferenceCode,
            Description = expense.Description,
            ApprovedByStaffUserId = expense.ApprovedByStaffUserId,
            ApprovedByStaffUserName = expense.ApprovedByStaffUser?.FullName,
            CreatedByStaffUserId = expense.CreatedByStaffUserId,
            CreatedByStaffUserName = expense.CreatedByStaffUser?.FullName,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
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

    private static string BuildCategoryCode(ExpenseCategory expenseType)
    {
        return Regex.Replace(expenseType.ToString(), "([a-z0-9])([A-Z])", "$1_$2")
            .ToUpperInvariant();
    }

    private static string BuildCategoryName(ExpenseCategory expenseType)
    {
        return Regex.Replace(expenseType.ToString(), "([a-z0-9])([A-Z])", "$1 $2");
    }
}
