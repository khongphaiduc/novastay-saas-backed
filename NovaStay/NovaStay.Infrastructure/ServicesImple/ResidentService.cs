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
    private readonly IMinioStorageService _storage;

    public ResidentService(IUnitOfWork unitOfWork, IMapper mapper, IMinioStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
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


        // fix exposed information 
        var ResidentTemp = residents.Select(r => new ResidentDto
        {
            Id = r.Id,          
            FullName = r.FullName.Value,
            Phone = r.Phone,
            ProfileImageUrl = "Mày xem cái gì ở đây",
            Address = "Mày xem cái gì ở đây",
            Email = "Mày xem cái gì ở đây",
            IdBackImageUrl = "Mày xem cái gì ở đây",
            IdentityCardNumber = "Mày xem cái gì ở đây",
            IdFrontImageUrl = "Không xem được đâu cưng",
            CreatedAt = DateTime.Now,
            Sex = "Mày xem cái gì ở đây"
        }).ToList();

        return ResidentTemp;
    }

    public async Task<IReadOnlyList<ResidentAccommodationDto>> GetActiveAccommodationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        return await _unitOfWork.ResidentMemberships.GetActiveAccommodationsByAccountIdAsync(
            accountId,
            cancellationToken);
    }

    public async Task<ResidentDto> UpdateResidentAsync(
        Guid residentId,
        UpdateResidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var resident = await _unitOfWork.Residents.GetByIdAsync(residentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resident with id {residentId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            resident.FullName = new EntityName(request.Name.Trim());
        if (!string.IsNullOrWhiteSpace(request.Sdt))
            resident.Phone = new PhoneNumber(request.Sdt.Trim());
        if (!string.IsNullOrWhiteSpace(request.IdentityCardNumber))
            resident.IdentityCardNumber = request.IdentityCardNumber.Trim();
        if (!string.IsNullOrWhiteSpace(request.Sex))
            resident.Sex = request.Sex.Trim();
        if (!string.IsNullOrWhiteSpace(request.Address))
            resident.Address = request.Address.Trim();
        if (request.Email != null) // Allow empty string to clear email if needed, or check IsNullOrWhiteSpace
            resident.Email = request.Email.Trim();

        _unitOfWork.Residents.Update(resident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResidentDto>(resident);
    }

    public async Task<ResidentDto> UploadImageAsync(
        Guid residentId,
        string imageType,
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        var resident = await _unitOfWork.Residents.GetByIdAsync(residentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resident with id {residentId} not found");

        switch (imageType.ToLowerInvariant())
        {
            case "front":
                resident.IdFrontImageUrl = imageUrl;
                break;
            case "back":
                resident.IdBackImageUrl = imageUrl;
                break;
            case "profile":
                resident.ProfileImageUrl = imageUrl;
                break;
            default:
                throw new ArgumentException("Invalid image type. Expected: 'front', 'back', 'profile'.");
        }

        _unitOfWork.Residents.Update(resident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResidentDto>(resident);
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

    // ─── TASK-053: Hồ sơ cá nhân cư dân ───────────────────────────────────────
    public async Task<ResidentProfileDto> GetMyProfileAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken)
            ?? throw new KeyNotFoundException("Resident profile not found.");

        var account = await _unitOfWork.Accounts.GetByIdAsync(resident.AccountId, cancellationToken);

        var membership = await _unitOfWork.ResidentMemberships.GetActiveByAccountIdAsync(accountId, cancellationToken);

        var profile = new ResidentProfileDto
        {
            ResidentId          = resident.Id,
            AccountId           = resident.AccountId,
            FullName            = resident.FullName.Value,
            Sex                 = resident.Sex,
            Phone               = resident.Phone.Value,
            Email               = resident.Email,
            Address             = resident.Address,
            IdentityCardNumber  = resident.IdentityCardNumber,
            IdFrontImageUrl     = resident.IdFrontImageUrl,
            IdBackImageUrl      = resident.IdBackImageUrl,
            ProfileImageUrl     = resident.ProfileImageUrl,
            MustSetPassword     = account?.MustSetPassword ?? false,
            CreatedAt           = resident.CreatedAt,
        };

        if (membership != null)
        {
            var org = await _unitOfWork.Organizations.GetByIdAsync(membership.OrganizationId, cancellationToken);
            profile.MembershipId     = membership.Id;
            profile.MembershipCode   = membership.MembershipCode;
            profile.MembershipStatus = membership.Status.Value;
            profile.OrganizationId   = membership.OrganizationId;
            profile.BusinessName     = org?.BusinessName.Value;
            profile.OwnerPhone       = org?.OwnerPhone.Value;
            profile.OwnerEmail       = org?.OwnerEmail.Value;
        }

        return profile;
    }

    // ─── TASK-049: Lấy thông tin phòng đang ở + ảnh ───────────────────────────
    public async Task<ResidentRoomDto?> GetMyRoomAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken);
        if (resident == null) return null;

        var membership = await _unitOfWork.ResidentMemberships.GetActiveByAccountIdAsync(accountId, cancellationToken);
        var organizationId = membership?.OrganizationId ?? Guid.Empty;

        // Tìm hợp đồng Active của cư dân để biết phòng đang ở
        var contracts = await _unitOfWork.Contracts.GetContractsWithDetailsAsync(
            organizationId: organizationId,
            residentId: resident.Id,
            status: "Active",
            cancellationToken: cancellationToken);

        var activeContract = contracts.FirstOrDefault();
        if (activeContract == null) return null;

        var roomDto = await _unitOfWork.Rooms.GetRoomWithImagesAsync(activeContract.RoomId, cancellationToken);
        if (roomDto == null) return null;

        var property = await _unitOfWork.Properties.GetByIdAsync(activeContract.PropertyId, cancellationToken);

        return new ResidentRoomDto
        {
            RoomId          = roomDto.Id,
            RoomNumber      = roomDto.RoomNumber,
            Floor           = roomDto.Floor,
            BasePrice       = roomDto.BasePrice,
            MaxOccupants    = roomDto.MaxOccupants,
            Status          = roomDto.Status,
            AmenitiesJson   = roomDto.AmenitiesJson,
            PropertyId      = activeContract.PropertyId,
            PropertyName    = property?.PropertyName,
            PropertyAddress = property?.Address,
            Images          = roomDto.Images?.Select(img => new RoomImageDto
            {
                Id       = img.Id,
                ImageUrl = img.ImageUrl,
                IsCover  = img.IsCover,
            }).ToList() ?? []
        };
    }

    // ─── TASK-051: Danh sách hợp đồng ─────────────────────────────────────────
    public async Task<IReadOnlyList<ContractDetailDto>> GetMyContractsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken);
        if (resident == null) return [];

        var membership = await _unitOfWork.ResidentMemberships.GetActiveByAccountIdAsync(accountId, cancellationToken);
        var organizationId = membership?.OrganizationId ?? Guid.Empty;

        return await _unitOfWork.Contracts.GetContractsWithDetailsAsync(
            organizationId: organizationId,
            residentId: resident.Id,
            cancellationToken: cancellationToken);
    }

    // ─── TASK-052: Danh sách hóa đơn ─────────────────────────────────────────────────────
    public async Task<IReadOnlyList<IncomeReceiptDto>> GetMyInvoicesAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken);
        if (resident == null) return [];

        // ✅ Fixed UnitOfWork pattern violation — dng qua _unitOfWork.IncomeReceipts thay vì inject trực tiếp
        return await _unitOfWork.IncomeReceipts.GetByResidentAsync(resident.Id, cancellationToken);
    }

    // ─── TASK-053: Cư dân tự cập nhật hồ sơ ─────────────────────────────────────────────
    public async Task<ResidentProfileDto> UpdateMyProfileAsync(
        Guid accountId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken)
            ?? throw new KeyNotFoundException("Resident not found.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            resident.FullName = new EntityName(request.FullName.Trim());
        if (!string.IsNullOrWhiteSpace(request.Phone))
            resident.Phone = new PhoneNumber(request.Phone.Trim());
        if (request.Email is not null)
            resident.Email = request.Email.Trim();
        if (request.Address is not null)
            resident.Address = request.Address.Trim();

        _unitOfWork.Residents.Update(resident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return full updated profile
        return await GetMyProfileAsync(accountId, cancellationToken);
    }

    // ─── TASK-053: Cư dân tự upload ảnh đại diện ───────────────────────────────────────
    public async Task<string> UploadMyAvatarAsync(
        Guid accountId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid access token.");

        var resident = await _unitOfWork.Residents.GetByAccountIdAsync(accountId, cancellationToken)
            ?? throw new KeyNotFoundException("Resident not found.");

        var imageUrl = await _storage.UploadImageAsync(fileStream, fileName, contentType, cancellationToken);

        resident.ProfileImageUrl = imageUrl;
        _unitOfWork.Residents.Update(resident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return imageUrl;
    }
}
