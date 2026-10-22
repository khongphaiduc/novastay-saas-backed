using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using Microsoft.EntityFrameworkCore;

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
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return;
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());
        var token = await _dbcontext.AccountRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAt == null, cancellationToken);
        if (token == null) return; // Repeated logout succeeds without changing another session.
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _dbcontext.SaveChangesAsync(cancellationToken);

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
