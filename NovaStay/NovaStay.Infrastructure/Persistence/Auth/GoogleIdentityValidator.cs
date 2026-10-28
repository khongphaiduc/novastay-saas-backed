using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.Persistence.Auth;

public sealed class GoogleIdentityValidator(IConfiguration configuration) : IGoogleIdentityValidator
{
    public async Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = configuration["GoogleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId)) throw new InvalidOperationException("Google sign-in is not configured.");
        if (string.IsNullOrWhiteSpace(idToken)) throw new UnauthorizedAccessException("Google token không hợp lệ hoặc đã hết hạn.");
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] }).WaitAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(payload.Subject) || !payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
                throw new UnauthorizedAccessException("Tài khoản Google chưa xác minh email.");
            return new(payload.Subject, payload.Email.Trim().ToLowerInvariant(), payload.Name ?? payload.Email);
        }
        catch (InvalidJwtException) { throw new UnauthorizedAccessException("Google token không hợp lệ hoặc đã hết hạn."); }
    }
}
