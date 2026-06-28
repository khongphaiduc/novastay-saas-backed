using NovaStay.Application.DTOs;
using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IContractRepository : IRepository<ContractEntity>
{
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