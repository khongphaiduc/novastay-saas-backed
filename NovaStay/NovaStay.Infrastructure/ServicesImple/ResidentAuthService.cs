using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Auth;
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
    private readonly IGoogleIdentityValidator _google;
    private readonly GoogleSignInCoordinator _coordinator;
    private readonly HostContext _context;

    public ResidentAuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IGoogleIdentityValidator google, GoogleSignInCoordinator coordinator, HostContext context)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _google = google; _coordinator = coordinator; _context = context;
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

    public async Task<RegisterResidentAccountResponse> RegisterAsync(
        RegisterResidentAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizeRequired(request.Phone, "Phone is required.");
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var fullName = NormalizeRequired(request.FullName, "Full name is required.");

        // Kiểm tra trùng SĐT
        var existingByPhone = await _unitOfWork.Accounts.GetByPhoneAsync(normalizedPhone, cancellationToken);
        if (existingByPhone is not null)
        {
            throw new InvalidOperationException("Số điện thoại đã được sử dụng.");
        }

        // Kiểm tra trùng Email
        var existingByEmail = await _unitOfWork.Accounts.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingByEmail is not null)
        {
            throw new InvalidOperationException("Email đã được sử dụng.");
        }

        var now = DateTime.UtcNow;

        // Tạo Account
        var account = new AccountEntity
        {
            Id = Guid.NewGuid(),
            AccountType = ResidentAccountType,
            CustomerName = fullName,
            Email = normalizedEmail,
            Phone = normalizedPhone,
            PasswordHash = HashPassword(request.Password),
            MustSetPassword = false,
            PasswordSetAt = now,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = now
        };

        // Tạo Resident profile
        var resident = new ResidentEntity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            FullName = fullName,
            Phone = normalizedPhone,
            Email = normalizedEmail,
            Sex = "Chưa cập nhật",
            CreatedAt = now
        };

        await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
        await _unitOfWork.Residents.AddAsync(resident, cancellationToken);

        // Tạo JWT token
        var tokens = _jwtTokenService.CreateTokenPair(
            account.Id,
            Guid.Empty,
            account.AccountType,
            account.CustomerName,
            account.Phone,
            account.Email,
            now);

        await _unitOfWork.AccountRefreshTokens.RevokeActiveByAccountIdAsync(
            account.Id, now, ipAddress, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResidentAccountResponse
        {
            AccountId = account.Id,
            ResidentId = resident.Id,
            AccountType = account.AccountType,
            Name = fullName,
            Phone = normalizedPhone,
            Email = normalizedEmail,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
        };
    }

    public async Task<LoginResidentAccountResponse> GoogleLoginAsync(
        GoogleLoginResidentRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var identity = await _google.ValidateAsync(request.IdToken, cancellationToken);
        using var lease = await _coordinator.EnterAsync(identity.Subject, cancellationToken);
        return await CompleteGoogleLoginAsync(identity, ipAddress, cancellationToken, true);
    }

    private async Task<LoginResidentAccountResponse> CompleteGoogleLoginAsync(
        GoogleIdentity identity, string? ipAddress, CancellationToken cancellationToken, bool retryUniqueConflict)
    {
        var googleId = identity.Subject;
        var email = identity.Email;
        var name = identity.Name;
        var now = DateTime.UtcNow;

        // 2. Tìm account theo GoogleId
        var account = await _unitOfWork.Accounts.GetByGoogleIdAsync(googleId, cancellationToken);
        ResidentEntity? resident = null;

        if (account is null && !string.IsNullOrEmpty(email))
        {
            // Thử tìm theo email (đã đăng ký bằng email/phone trước đó)
            account = await _unitOfWork.Accounts.GetByEmailAsync(email, cancellationToken);

            if (account is not null && account.AccountType == ResidentAccountType)
            {
                if (account.IsActive == false || !string.IsNullOrEmpty(account.GoogleId) && account.GoogleId != googleId)
                    throw new UnauthorizedAccessException("Không thể đăng nhập bằng tài khoản Google này.");
                // Liên kết Google vào account hiện tại
                account.GoogleId = googleId;
                account.ExternalProvider = "Google";
                account.IsEmailVerified = true;
                account.LastLoginAt = now;

            }
            else if (account is not null)
            {
                throw new InvalidOperationException("Email đã được sử dụng cho tài khoản khác.");
            }
        }

        if (account != null && (account.AccountType != ResidentAccountType || account.IsActive == false ||
            !string.IsNullOrEmpty(account.GoogleId) && account.GoogleId != googleId))
            throw new UnauthorizedAccessException("Không thể đăng nhập bằng tài khoản Google này.");
        if (account is null)
        {
            // 3. Tạo account + resident mới
            account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                AccountType = ResidentAccountType,
                CustomerName = name,
                Email = email,
                Phone = string.Empty,
                GoogleId = googleId,
                ExternalProvider = "Google",
                IsEmailVerified = true,
                MustSetPassword = true,
                IsActive = true,
                LastLoginAt = now,
                CreatedAt = now
            };

            resident = new ResidentEntity
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                FullName = name,
                Phone = string.Empty,
                Email = email ?? string.Empty,
                Sex = "Chưa cập nhật",
                CreatedAt = now
            };

            await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
            await _unitOfWork.Residents.AddAsync(resident, cancellationToken);
        }
        else
        {
            account.LastLoginAt = now;
            _unitOfWork.Accounts.Update(account);
        }

        // 4. Lấy Resident profile
        resident ??= await _unitOfWork.Residents.GetByAccountIdAsync(account.Id, cancellationToken);
        if (resident is null)
        {
            throw new InvalidOperationException("Không tìm thấy thông tin cư dân.");
        }

        // 5. Lấy membership (nếu có)
        var membership = await _unitOfWork.ResidentMemberships.GetActiveByAccountIdAsync(
            account.Id, cancellationToken);
        var organizationId = membership?.OrganizationId ?? Guid.Empty;

        // 6. Tạo JWT tokens
        var tokens = _jwtTokenService.CreateTokenPair(
            account.Id,
            organizationId,
            account.AccountType,
            account.CustomerName,
            account.Phone,
            account.Email,
            now);

        await _unitOfWork.AccountRefreshTokens.RevokeActiveByAccountIdAsync(
            account.Id, now, ipAddress, cancellationToken);

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

        try { await _unitOfWork.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException error) when (retryUniqueConflict && error.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        {
            // SaveChanges is atomic. Discard this request's failed graph before reading the winning signup.
            _context.ChangeTracker.Clear();
            var winner = await _unitOfWork.Accounts.GetByGoogleIdAsync(googleId, cancellationToken);
            if (winner == null) throw new InvalidOperationException("Tài khoản đang được xử lý. Vui lòng thử lại.");
            return await CompleteGoogleLoginAsync(identity, ipAddress, cancellationToken, false);
        }

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
}
