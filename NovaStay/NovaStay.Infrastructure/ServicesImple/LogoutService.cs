using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class LogoutService : ILogoutService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutService(IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task LogoutAsync(
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());

        var revoked = await _unitOfWork.AccountRefreshTokens.RevokeByTokenHashAsync(
            tokenHash,
            DateTime.UtcNow,
            ipAddress,
            cancellationToken);

        if (revoked)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
