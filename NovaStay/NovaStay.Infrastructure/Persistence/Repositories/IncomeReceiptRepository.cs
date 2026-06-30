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

    public Task<bool> PropertyExistsAsync(
        Guid organizationId,
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        return _context.Properties
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == propertyId && entity.OrganizationId == organizationId,
                cancellationToken);
    }

    public Task<bool> ReceiptNumberExistsAsync(
        Guid organizationId,
        string receiptNumber,
        CancellationToken cancellationToken = default)
    {
        return _context.IncomeReceipts
            .AsNoTracking()
            .AnyAsync(
                entity => entity.OrganizationId == organizationId && entity.ReceiptNumber == receiptNumber,
                cancellationToken);
    }

    public Task<bool> RoomExistsAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return _context.Rooms
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == roomId && entity.PropertyId == propertyId,
                cancellationToken);
    }

    public Task<bool> ResidentExistsAsync(
        Guid residentId,
        CancellationToken cancellationToken = default)
    {
        return _context.Residents
            .AsNoTracking()
            .AnyAsync(entity => entity.Id == residentId, cancellationToken);
    }

    public Task<bool> StaffUserExistsAsync(
        Guid organizationId,
        Guid staffUserId,
        CancellationToken cancellationToken = default)
    {
        return _context.StaffUsers
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == staffUserId && entity.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<IncomeReceiptDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await PropertyExistsAsync(organizationId, propertyId, cancellationToken);

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

    public async Task<IncomeReceiptDto?> GetByIdAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeReceiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _context.IncomeReceipts
            .AsNoTracking()
            .Include(entity => entity.IncomeCategory)
            .Include(entity => entity.Room)
            .Include(entity => entity.Resident)
            .Include(entity => entity.CollectedByStaffUser)
            .FirstOrDefaultAsync(
                entity => entity.Id == incomeReceiptId
                    && entity.OrganizationId == organizationId
                    && entity.PropertyId == propertyId,
                cancellationToken);

        return receipt is null ? null : MapToDto(receipt);
    }

    public async Task<Guid> EnsureIncomeCategoryAsync(
        Guid organizationId,
        Guid? incomeCategoryId,
        IncomeCategory incomeType,
        CancellationToken cancellationToken = default)
    {
        if (incomeCategoryId.HasValue)
        {
            var existingCategoryId = await _context.IncomeCategories
                .AsNoTracking()
                .Where(entity => entity.Id == incomeCategoryId.Value && entity.OrganizationId == organizationId)
                .Select(entity => (Guid?)entity.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingCategoryId.HasValue)
            {
                return existingCategoryId.Value;
            }
        }

        var categoryCode = BuildCategoryCode(incomeType);
        var categoryName = BuildCategoryName(incomeType);

        var existingCategory = await _context.IncomeCategories
            .FirstOrDefaultAsync(
                entity => entity.OrganizationId == organizationId && entity.CategoryCode == categoryCode,
                cancellationToken);

        if (existingCategory is not null)
        {
            return existingCategory.Id;
        }

        var category = new Models.IncomeCategory
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

        return category.Id;
    }

    public async Task<IncomeReceiptDto> AddAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeCategoryId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var receipt = new Models.IncomeReceipt
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            IncomeCategoryId = incomeCategoryId,
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

    public async Task<IncomeReceiptDto> UpdateStatusAsync(
        Guid organizationId,
        Guid propertyId,
        Guid incomeReceiptId,
        ApprovalStatus status,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _context.IncomeReceipts
            .Include(entity => entity.IncomeCategory)
            .Include(entity => entity.Room)
            .Include(entity => entity.Resident)
            .Include(entity => entity.CollectedByStaffUser)
            .FirstOrDefaultAsync(
                entity => entity.Id == incomeReceiptId
                    && entity.OrganizationId == organizationId
                    && entity.PropertyId == propertyId,
                cancellationToken);

        if (receipt is null)
        {
            throw new KeyNotFoundException("Income receipt not found in property.");
        }

        if (!string.Equals(receipt.Status, ApprovalStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only income receipts with Pending status can be updated.");
        }

        receipt.Status = status.ToString();
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(receipt);
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
