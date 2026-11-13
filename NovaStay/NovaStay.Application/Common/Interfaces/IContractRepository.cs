using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IContractRepository : IRepository<ContractEntity>
{
    Task<PagedResult<ContractDetailDto>> GetPagedContractsAsync(Guid organizationId, string? search, string? status, Guid? residentId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> HasOtherActiveContractForRoomAsync(Guid roomId, Guid excludeContractId, CancellationToken cancellationToken = default);
    Task<ContractDetailDto?> FindActiveCoResidentContractAsync(Guid organizationId, Guid residentId, CancellationToken cancellationToken = default);
    // Hợp đồng Active mà cư dân là người đại diện hoặc đang ở ghép (ưu tiên người đại diện)
    Task<ContractDetailDto?> FindActiveContractOfResidentAsync(Guid organizationId, Guid residentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContractDetailDto>> GetContractsWithDetailsAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        Guid? residentId = null,
        CancellationToken cancellationToken = default);

    Task<ContractDetailDto?> GetContractDetailByIdAsync(
        Guid contractId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContractDetailDto>> GetExpiringSoonAsync(
        Guid organizationId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContractEntity>> GetAllActiveExpiringOnDateAsync(
        DateTime targetDate,
        CancellationToken cancellationToken = default);
}
