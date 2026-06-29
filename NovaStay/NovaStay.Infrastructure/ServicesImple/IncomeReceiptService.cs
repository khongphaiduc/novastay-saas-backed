using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Enums;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class IncomeReceiptService : IIncomeReceiptService
{
    private readonly IIncomeReceiptRepository _incomeReceiptRepository;

    public IncomeReceiptService(IIncomeReceiptRepository incomeReceiptRepository)
    {
        _incomeReceiptRepository = incomeReceiptRepository;
    }

    public Task<IReadOnlyList<IncomeReceiptDto>?> GetByPropertyAsync(
        Guid organizationId,
        Guid propertyId,
        ApprovalStatus? status = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default)
    {
        return _incomeReceiptRepository.GetByPropertyAsync(
            organizationId,
            propertyId,
            status,
            paymentMethod,
            cancellationToken);
    }

    public async Task<IncomeReceiptDto> CreateAsync(
        Guid organizationId,
        Guid propertyId,
        CreateIncomeReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
        {
            throw new ArgumentException("ReceiptNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PayerName))
        {
            throw new ArgumentException("PayerName is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0.");
        }

        var propertyExists = await _incomeReceiptRepository.PropertyExistsAsync(
            organizationId,
            propertyId,
            cancellationToken);

        if (!propertyExists)
        {
            throw new KeyNotFoundException("Property not found in organization.");
        }

        request.ReceiptNumber = request.ReceiptNumber.Trim();
        request.PayerName = request.PayerName.Trim();
        request.ReferenceCode = string.IsNullOrWhiteSpace(request.ReferenceCode) ? null : request.ReferenceCode.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        var duplicatedReceiptNumber = await _incomeReceiptRepository.ReceiptNumberExistsAsync(
            organizationId,
            request.ReceiptNumber,
            cancellationToken);

        if (duplicatedReceiptNumber)
        {
            throw new InvalidOperationException("Receipt number already exists in organization.");
        }

        if (request.RoomId.HasValue)
        {
            var roomExists = await _incomeReceiptRepository.RoomExistsAsync(
                propertyId,
                request.RoomId.Value,
                cancellationToken);

            if (!roomExists)
            {
                throw new KeyNotFoundException("Room not found in property.");
            }
        }

        if (request.ResidentId.HasValue)
        {
            var residentExists = await _incomeReceiptRepository.ResidentExistsAsync(
                request.ResidentId.Value,
                cancellationToken);

            if (!residentExists)
            {
                throw new KeyNotFoundException("Resident not found.");
            }
        }

        if (request.CollectedByStaffUserId.HasValue)
        {
            var staffUserExists = await _incomeReceiptRepository.StaffUserExistsAsync(
                organizationId,
                request.CollectedByStaffUserId.Value,
                cancellationToken);

            if (!staffUserExists)
            {
                throw new KeyNotFoundException("CollectedByStaffUser not found in organization.");
            }
        }

        var incomeCategoryId = await _incomeReceiptRepository.EnsureIncomeCategoryAsync(
            organizationId,
            request.IncomeCategoryId,
            request.IncomeType,
            cancellationToken);

        return await _incomeReceiptRepository.AddAsync(
            organizationId,
            propertyId,
            incomeCategoryId,
            request,
            cancellationToken);
    }
}
