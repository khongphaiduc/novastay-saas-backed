using MassTransit;
using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using System.Security.Cryptography;

namespace NovaStay.Infrastructure.Persistence.Auth;

internal sealed class AuthService : IAuthService
{
    private const string BusinessOwnerAccountType = "BusinessOwner";
    private const string InvalidLoginMessage = "Invalid email or password.";
    private const int TemporaryPasswordLength = 12;
    private const string TemporaryPasswordCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
    private readonly HostContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPublishEndpoint _rabbitMQ;

    public AuthService(HostContext context, IJwtTokenService jwtTokenService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _rabbitMQ = publishEndpoint;
    }

    public async Task<RegisterOrganizationAccountResponse> RegisterOrganizationOwnerAsync(
        RegisterOrganizationAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = request.Phone.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var accountExists = await _context.Accounts
            .AnyAsync(
                account => account.Phone == normalizedPhone || account.Email == normalizedEmail,
                cancellationToken);

        if (accountExists)
        {
            throw new InvalidOperationException("Phone or email already exists.");
        }

        var ownerEmailExists = await _context.Organizations
            .AnyAsync(
                organization => organization.OwnerEmail == normalizedEmail,
                cancellationToken);

        if (ownerEmailExists)
        {
            throw new InvalidOperationException("Owner email already exists.");
        }

        var package = await GetDefaultPackageAsync(cancellationToken);
        if (package is null)
        {
            throw new InvalidOperationException("No subscription package is configured.");
        }

        var now = DateTime.UtcNow;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountType = BusinessOwnerAccountType,
            CustomerName = request.CustomerName.Trim(),
            Email = normalizedEmail,
            Phone = normalizedPhone,
            PasswordHash = HashPassword(request.Password),
            MustSetPassword = false,
            PasswordSetAt = now,
            IsActive = true,
            CreatedAt = now
        };

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            OwnerAccountId = account.Id,
            PackageId = package.Id,
            BusinessName = request.BusinessName.Trim(),
            BusinessArea = request.BusinessArea.Trim(),
            OwnerEmail = normalizedEmail,
            OwnerPhone = normalizedPhone,
            SubscriptionStatus = package.PackageKey == "TRIAL" ? "Trial" : "Active",
            TokenBalance = package.TokenGiftMonthly ?? 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        var tokens = _jwtTokenService.CreateTokenPair(
            account.Id,
            organization.Id,
            account.AccountType,
            account.CustomerName,
            account.Phone,
            account.Email,
            now);

        var refreshTokenEntity = new AccountRefreshToken
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedAt = now,
            CreatedByIp = ipAddress
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.Organizations.AddAsync(organization, cancellationToken);
        await _context.AccountRefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _rabbitMQ.Publish(new NofiticationRegisterBusiness
        {
            BusinessName = organization.BusinessName,
            BusinessEmail = organization.OwnerEmail,
            BusinessPhone = organization.OwnerPhone,
            BusinessCity = request.BusinessArea,
            BusinessZipCode = "Chưa cập nhật",
            BusinessCountry = "Việt Nam",
            AccountName = account.CustomerName,
            Password = request.Password
        }, cancellationToken);

        return new RegisterOrganizationAccountResponse
        {
            AccountId = account.Id,
            OrganizationId = organization.Id,
            AccountType = account.AccountType,
            CustomerName = account.CustomerName,
            Phone = account.Phone,
            Email = normalizedEmail,
            BusinessArea = organization.BusinessArea,
            BusinessName = organization.BusinessName,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
        };
    }

    public async Task<LoginBusinessAccountResponse> LoginBusinessOwnerAsync(LoginBusinessAccountRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var account = await _context.Accounts
            .FirstOrDefaultAsync(
                entity => entity.Email == normalizedEmail
                    && entity.AccountType == BusinessOwnerAccountType,
                cancellationToken);

        if (account is null
            || account.IsActive == false
            || string.IsNullOrWhiteSpace(account.PasswordHash)
            || !VerifyPassword(request.Password, account.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidLoginMessage);
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(
                entity => entity.OwnerAccountId == account.Id,
                cancellationToken);

        if (organization is null)
        {
            throw new UnauthorizedAccessException(InvalidLoginMessage);
        }

        var now = DateTime.UtcNow;
        account.LastLoginAt = now;

        var tokens = _jwtTokenService.CreateTokenPair(
            account.Id,
            organization.Id,
            account.AccountType,
            account.CustomerName,
            account.Phone,
            account.Email,
            now);

        var activeRefreshTokens = await _context.AccountRefreshTokens
            .Where(token => token.AccountId == account.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = now;
            refreshToken.RevokedByIp = ipAddress;
        }

        var refreshTokenEntity = new AccountRefreshToken
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedAt = now,
            CreatedByIp = ipAddress
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.AccountRefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new LoginBusinessAccountResponse
        {
            AccountId = account.Id,
            OrganizationId = organization.Id,
            AccountType = account.AccountType,
            CustomerName = account.CustomerName,
            Phone = account.Phone,
            Email = account.Email ?? string.Empty,
            BusinessArea = organization.BusinessArea,
            BusinessName = organization.BusinessName,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
        };
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeResetPasswordEmail(request.Email);
        var account = await _context.Accounts
            .FirstOrDefaultAsync(
                entity => entity.Email == normalizedEmail,
                cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException("Account email was not found.");
        }

        if (account.IsActive == false)
        {
            throw new InvalidOperationException("Account is inactive.");
        }

        var now = DateTime.UtcNow;
        var temporaryPassword = GenerateTemporaryPassword();
        account.PasswordHash = HashPassword(temporaryPassword);
        account.MustSetPassword = true;
        account.PasswordSetAt = now;

        var activeRefreshTokens = await _context.AccountRefreshTokens
            .Where(token => token.AccountId == account.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = now;
            refreshToken.RevokedByIp = ipAddress;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _rabbitMQ.Publish(new NofiticationResetPassword
        {
            Email = normalizedEmail,
            NewPassword = temporaryPassword
        }, cancellationToken);
    }

    private async Task<SubscriptionPackage?> GetDefaultPackageAsync(CancellationToken cancellationToken)
    {
        return await _context.SubscriptionPackages
            .OrderByDescending(package => package.PackageKey == "TRIAL")
            .ThenBy(package => package.PriceMonthly)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string NormalizeResetPasswordEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static string GenerateTemporaryPassword()
    {
        Span<char> password = stackalloc char[TemporaryPasswordLength];
        for (var index = 0; index < password.Length; index++)
        {
            var characterIndex = RandomNumberGenerator.GetInt32(TemporaryPasswordCharacters.Length);
            password[index] = TemporaryPasswordCharacters[characterIndex];
        }

        return new string(password);
    }

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"pbkdf2-sha256${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
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
