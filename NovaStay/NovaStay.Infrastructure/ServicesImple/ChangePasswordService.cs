using System.Security.Cryptography;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ChangePasswordService : IChangePasswordService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ChangePasswordAsync(
        Guid accountId,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var currentPassword = request.CurrentPassword.Trim();
        var newPassword = request.NewPassword.Trim();
        var confirmNewPassword = request.ConfirmNewPassword.Trim();

        if (string.IsNullOrWhiteSpace(currentPassword)
            || string.IsNullOrWhiteSpace(newPassword)
            || string.IsNullOrWhiteSpace(confirmNewPassword))
        {
            throw new InvalidOperationException("Password fields are required.");
        }

        if (newPassword != confirmNewPassword)
        {
            throw new InvalidOperationException("New password and confirmation password do not match.");
        }

        if (newPassword.Length < 8)
        {
            throw new InvalidOperationException("New password must be at least 8 characters.");
        }

        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);

        if (account is null || account.IsActive == false)
        {
            throw new UnauthorizedAccessException("Account is not available.");
        }

        if (string.IsNullOrWhiteSpace(account.PasswordHash)
            || !VerifyPassword(currentPassword, account.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        if (VerifyPassword(newPassword, account.PasswordHash))
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }

        var now = DateTime.UtcNow;
        account.PasswordHash = HashPassword(newPassword);
        account.MustSetPassword = false;
        account.PasswordSetAt = now;

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.AccountRefreshTokens.RevokeActiveByAccountIdAsync(
            account.Id,
            now,
            ipAddress,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
