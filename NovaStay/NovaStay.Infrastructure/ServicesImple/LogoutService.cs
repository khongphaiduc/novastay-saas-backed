using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class LogoutService : ILogoutService
{
    private readonly HostContext _dbcontext;
    private readonly IJwtTokenService _jwtTokenService;

    public LogoutService(IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork, HostContext hostContext)
    {
        _dbcontext = hostContext;
        _jwtTokenService = jwtTokenService;

    }

    public async Task LogoutAsync(
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());

        var user = _dbcontext.AccountRefreshTokens.Where(s => s.TokenHash == tokenHash && s.RevokedAt == null).Select(t => new InforUser
        {
            AccountId = t.AccountId,
            OrganizationId = t.Account.OwnedOrganizations.FirstOrDefault().Id,
            AccountType = t.Account.AccountType,
            UserName = t.Account.CustomerName,
            Phone = t.Account.Phone,
            Email = t.Account.Email,
        }).FirstOrDefault();

    }


}


public class InfoUser
{
    public Guid AccountId { get; set; }
    public Guid OrganizationId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
