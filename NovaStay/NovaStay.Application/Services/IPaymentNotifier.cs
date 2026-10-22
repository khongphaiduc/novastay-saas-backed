using NovaStay.Application.DTOs;

namespace NovaStay.Application.Services;

public interface IPaymentNotifier
{
    Task NotifyAsync(Guid ownerAccountId, Guid? residentAccountId, InvoicePaymentUpdated message, CancellationToken ct);
}
