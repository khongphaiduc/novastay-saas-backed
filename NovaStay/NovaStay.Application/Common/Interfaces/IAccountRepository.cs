using NovaStay.Domain.Entities;

namespace NovaStay.Application.Common.Interfaces;

public interface IAccountRepository : IRepository<AccountEntity>
{
    Task<AccountEntity?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<AccountEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

}
