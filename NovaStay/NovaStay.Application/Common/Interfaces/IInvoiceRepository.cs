using NovaStay.Domain.Entities;
namespace NovaStay.Application.Common.Interfaces;
public interface IInvoiceRepository : IRepository<InvoiceEntity>
{
    // Hóa đơn của cư dân trong cơ sở đang chờ thanh toán hoặc chờ xác nhận
    Task<(int Count, decimal Total)> GetOpenInvoiceSummaryForResidentAsync(Guid organizationId, Guid residentId, CancellationToken cancellationToken = default);
}