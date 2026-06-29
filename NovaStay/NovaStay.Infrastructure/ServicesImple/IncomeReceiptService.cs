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

    public Task<IncomeReceiptDto> CreateAsync(
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

        request.ReceiptNumber = request.ReceiptNumber.Trim();
        request.PayerName = request.PayerName.Trim();
        request.ReferenceCode = string.IsNullOrWhiteSpace(request.ReferenceCode) ? null : request.ReferenceCode.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        return _incomeReceiptRepository.CreateAsync(
            organizationId,
            propertyId,
            request,
            cancellationToken);
    }
}
