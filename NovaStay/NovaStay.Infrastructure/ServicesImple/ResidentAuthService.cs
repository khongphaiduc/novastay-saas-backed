using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using System.Security.Cryptography;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ResidentAuthService : IResidentAuthService
{
    private const string ResidentAccountType = "Resident";
    private const string InvalidLoginMessage = "Invalid phone or password.";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public ResidentAuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResidentAccountResponse> LoginAsync(
        LoginResidentAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var phone = NormalizeRequired(request.Sdt, InvalidLoginMessage);
        var password = NormalizeRequired(request.Password, InvalidLoginMessage);

        var account = await _unitOfWork.Accounts.GetByPhoneAsync(phone, cancellationToken);
        if (account is null
            || account.AccountType != ResidentAccountType
            || account.IsActive == false
            || string.IsNullOrWhiteSpace(account.PasswordHash)
            || !VerifyPassword(password, account.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidLoginMessage);
        }

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(account.Id, cancellationToken);
        if (resident is null)
        {
            throw new UnauthorizedAccessException(InvalidLoginMessage);
        }

        var membership = await _unitOfWork.ResidentMemberships.GetActiveByAccountIdAsync(
            account.Id,
            cancellationToken);
        var organizationId = membership?.OrganizationId ?? Guid.Empty;
        var now = DateTime.UtcNow;

        account.LastLoginAt = now;

        var tokens = _jwtTokenService.CreateTokenPair(
            account.Id,
            organizationId,
            account.AccountType,
            account.CustomerName,
            account.Phone,
            account.Email,
            now);

        await _unitOfWork.AccountRefreshTokens.RevokeActiveByAccountIdAsync(
            account.Id,
            now,
            ipAddress,
            cancellationToken);

        await _unitOfWork.AccountRefreshTokens.AddAsync(
            new AccountRefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                TokenHash = tokens.RefreshTokenHash,
                ExpiresAt = tokens.RefreshTokenExpiresAt,
                CreatedAt = now,
                CreatedByIp = ipAddress
            },
            cancellationToken);

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResidentAccountResponse
        {
            AccountId = account.Id,
            ResidentId = resident.Id,
            OrganizationId = organizationId,
            AccountType = account.AccountType,
            Name = resident.FullName.Value,
            Sdt = resident.Phone.Value,
            Email = resident.Email,
            IdentityCardNumber = resident.IdentityCardNumber,
            Sex = resident.Sex,
            Address = resident.Address,
            MustSetPassword = account.MustSetPassword == true,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
        };
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UnauthorizedAccessException(errorMessage);
        }

        return value.Trim();
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
