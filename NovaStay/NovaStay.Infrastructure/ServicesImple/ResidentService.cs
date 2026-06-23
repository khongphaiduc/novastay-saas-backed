using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;
using System.Security.Cryptography;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ResidentService : IResidentService
{
    private const string ResidentAccountType = "Resident";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ResidentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CreateResidentAccountResponse> CreateResidentAccountAsync(
        CreateResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequired(request.Name, "Name is required.");
        var phone = NormalizeRequired(request.Sdt, "Sdt is required.");
        var identityCardNumber = NormalizeRequired(
            request.IdentityCardNumber,
            "Identity card number is required.");
        var sex = NormalizeRequired(request.Sex, "Sex is required.");
        var address = NormalizeRequired(request.Address, "Address is required.");

        if (identityCardNumber.Length < 8 || identityCardNumber.Any(character => !char.IsDigit(character)))
        {
            throw new InvalidOperationException("Identity card number must contain at least 8 digits.");
        }

        var existingAccount = await _unitOfWork.Accounts.GetByPhoneAsync(phone, cancellationToken);
        if (existingAccount is not null)
        {
            throw new InvalidOperationException("Phone already exists.");
        }

        var existingResident = await _unitOfWork.Residents.GetByIdentityCardNumberAsync(
            identityCardNumber,
            cancellationToken);
        if (existingResident is not null)
        {
            throw new InvalidOperationException("Identity card number already exists.");
        }

        var now = DateTime.UtcNow;
        var accountId = Guid.NewGuid();
        var residentId = Guid.NewGuid();
        var defaultPassword = identityCardNumber[^8..];

        var account = new AccountEntity
        {
            Id = accountId,
            AccountType = ResidentAccountType,
            CustomerName = name,
            Phone = phone,
            PasswordHash = HashPassword(defaultPassword),
            MustSetPassword = true,
            IsActive = true,
            CreatedAt = now
        };

        var resident = new ResidentEntity
        {
            Id = residentId,
            AccountId = accountId,
            FullName = new EntityName(name),
            Sex = sex,
            Phone = new PhoneNumber(phone),
            Address = address,
            IdentityCardNumber = identityCardNumber,
            CreatedAt = now
        };

        await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
        await _unitOfWork.Residents.AddAsync(resident, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateResidentAccountResponse
        {
            AccountId = accountId,
            ResidentId = residentId,
            AccountType = account.AccountType,
            Name = name,
            Sdt = phone,
            IdentityCardNumber = identityCardNumber,
            Sex = sex,
            Address = address,
            MustSetPassword = account.MustSetPassword == true
        };
    }

    public async Task<IReadOnlyList<ResidentDto>> SearchByPhoneAsync(
        string? phone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return [];
        }

        var residents = await _unitOfWork.Residents.SearchByPhoneAsync(
            phone.Trim(),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ResidentDto>>(residents);
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
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
}
