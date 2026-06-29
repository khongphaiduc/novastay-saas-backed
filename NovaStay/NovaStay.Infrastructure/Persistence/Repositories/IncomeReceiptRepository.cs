using Microsoft.EntityFrameworkCore;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Domain.Enums;
using NovaStay.Infrastructure.ContextDB;
using System.Text.RegularExpressions;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal sealed class IncomeReceiptRepository : IIncomeReceiptRepository
{
    private readonly HostContext _context;

    public IncomeReceiptRepository(HostContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<IncomeReceiptDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(entity => entity.Id == propertyId && entity.OrganizationId == organizationId, cancellationToken);

        if (!propertyExists)
        {
            return null;
        }

        var query = _context.IncomeReceipts
            .AsNoTracking()
            .Include(receipt => receipt.IncomeCategory)
            .Include(receipt => receipt.Room)
            .Include(receipt => receipt.Resident)
            .Include(receipt => receipt.CollectedByStaffUser)
            .Where(receipt => receipt.OrganizationId == organizationId && receipt.PropertyId == propertyId);

        if (status.HasValue)
        {
            var statusValue = status.Value.ToString();
            query = query.Where(receipt => receipt.Status == statusValue);
        }

        if (paymentMethod.HasValue)
        {
            var paymentMethodValue = paymentMethod.Value.ToString();
            query = query.Where(receipt => receipt.PaymentMethod == paymentMethodValue);
        }

        var receipts = await query
            .OrderByDescending(receipt => receipt.CollectedAt)
            .ThenByDescending(receipt => receipt.CreatedAt)
            .ToListAsync(cancellationToken);

        return receipts.Select(MapToDto).ToList();
    }

    public async Task<IncomeReceiptDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(entity => entity.Id == propertyId && entity.OrganizationId == organizationId, cancellationToken);

        if (!propertyExists)
        {
            throw new KeyNotFoundException("Property not found in organization.");
        }

        var duplicatedReceiptNumber = await _context.IncomeReceipts
            .AsNoTracking()
            .AnyAsync(entity => entity.OrganizationId == organizationId && entity.ReceiptNumber == request.ReceiptNumber, cancellationToken);

        if (duplicatedReceiptNumber)
        {
            throw new InvalidOperationException("Receipt number already exists in organization.");
        }

        if (request.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms
                .AsNoTracking()
                .AnyAsync(entity => entity.Id == request.RoomId.Value && entity.PropertyId == propertyId, cancellationToken);

            if (!roomExists)
            {
                throw new KeyNotFoundException("Room not found in property.");
            }
        }

        if (request.ResidentId.HasValue)
        {
            var residentExists = await _context.Residents
                .AsNoTracking()
                .AnyAsync(entity => entity.Id == request.ResidentId.Value, cancellationToken);

            if (!residentExists)
            {
                throw new KeyNotFoundException("Resident not found.");
            }
        }

        if (request.CollectedByStaffUserId.HasValue)
        {
            var staffUserExists = await _context.StaffUsers
                .AsNoTracking()
                .AnyAsync(entity => entity.Id == request.CollectedByStaffUserId.Value && entity.OrganizationId == organizationId, cancellationToken);

            if (!staffUserExists)
            {
                throw new KeyNotFoundException("CollectedByStaffUser not found in organization.");
            }
        }

        var category = await ResolveIncomeCategoryAsync(organizationId, request, cancellationToken);

        var receipt = new Models.IncomeReceipt
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            IncomeCategoryId = category.Id,
            PropertyId = propertyId,
            RoomId = request.RoomId,
            ResidentId = request.ResidentId,
            CollectedByStaffUserId = request.CollectedByStaffUserId,
            ReceiptNumber = request.ReceiptNumber,
            IncomeType = request.IncomeType.ToString(),
            PayerName = request.PayerName,
            Amount = request.Amount,
            CollectedAt = request.CollectedAt,
            PaymentMethod = request.PaymentMethod.ToString(),
            Status = request.Status.ToString(),
            ReferenceCode = request.ReferenceCode,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.IncomeReceipts.AddAsync(receipt, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var createdReceipt = await _context.IncomeReceipts
            .AsNoTracking()
            .Include(entity => entity.IncomeCategory)
            .Include(entity => entity.Room)
            .Include(entity => entity.Resident)
            .Include(entity => entity.CollectedByStaffUser)
            .FirstAsync(entity => entity.Id == receipt.Id, cancellationToken);

        return MapToDto(createdReceipt);
    }

    private async Task<Models.IncomeCategory> ResolveIncomeCategoryAsync(
        Guid organizationId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken)
    {
        if (request.IncomeCategoryId.HasValue)
        {
            var existingCategory = await _context.IncomeCategories
                .FirstOrDefaultAsync(
                    entity => entity.Id == request.IncomeCategoryId.Value && entity.OrganizationId == organizationId,
                    cancellationToken);

            if (existingCategory is not null)
            {
                return existingCategory;
            }
        }

        var categoryCode = BuildCategoryCode(request.IncomeType);
        var categoryName = BuildCategoryName(request.IncomeType);

        var category = await _context.IncomeCategories
            .FirstOrDefaultAsync(
                entity => entity.OrganizationId == organizationId && entity.CategoryCode == categoryCode,
                cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = new Models.IncomeCategory
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.IncomeCategories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    private static IncomeReceiptDto MapToDto(Models.IncomeReceipt receipt)
    {
        return new IncomeReceiptDto
        {
            Id = receipt.Id,
            OrganizationId = receipt.OrganizationId,
            PropertyId = receipt.PropertyId,
            IncomeCategoryId = receipt.IncomeCategoryId,
            IncomeCategoryCode = receipt.IncomeCategory.CategoryCode,
            IncomeCategoryName = receipt.IncomeCategory.CategoryName,
            RoomId = receipt.RoomId,
            RoomNumber = receipt.Room?.RoomNumber,
            ResidentId = receipt.ResidentId,
            ResidentName = receipt.Resident?.FullName,
            ReceiptNumber = receipt.ReceiptNumber,
            IncomeType = TryParseEnum<IncomeCategory>(receipt.IncomeType),
            IncomeTypeRaw = receipt.IncomeType,
            PayerName = receipt.PayerName,
            Amount = receipt.Amount,
            CollectedAt = receipt.CollectedAt,
            PaymentMethod = TryParseEnum<PaymentMethod>(receipt.PaymentMethod),
            PaymentMethodRaw = receipt.PaymentMethod,
            Status = TryParseEnum<ApprovalStatus>(receipt.Status),
            StatusRaw = receipt.Status,
            CollectedByStaffUserId = receipt.CollectedByStaffUserId,
            CollectedByStaffUserName = receipt.CollectedByStaffUser?.FullName,
            ReferenceCode = receipt.ReferenceCode,
            Description = receipt.Description,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt
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

    private static string BuildCategoryCode(IncomeCategory incomeType)
    {
        return Regex.Replace(incomeType.ToString(), "([a-z0-9])([A-Z])", "$1_$2")
            .ToUpperInvariant();
    }

    private static string BuildCategoryName(IncomeCategory incomeType)
    {
        return Regex.Replace(incomeType.ToString(), "([a-z0-9])([A-Z])", "$1 $2");
    }
}
