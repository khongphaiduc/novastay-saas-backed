using NovaStay.Domain.Entities;
namespace NovaStay.Application.Common.Interfaces;
public interface IResidentRepository : IRepository<ResidentEntity>
{
    Task<IReadOnlyList<ResidentEntity>> SearchByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default);
}
