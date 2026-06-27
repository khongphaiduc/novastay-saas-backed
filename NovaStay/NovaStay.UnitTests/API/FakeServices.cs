using NovaStay.Application.DTOs;
using NovaStay.Application.Services;

namespace NovaStay.UnitTests.API;

internal sealed class FakeAuthService : IAuthService
{
    public RegisterOrganizationAccountResponse RegisterResponse { get; set; } = new();
    public LoginBusinessAccountResponse LoginBusinessResponse { get; set; } = new();
    public Exception? RegisterException { get; set; }
    public Exception? LoginBusinessException { get; set; }
    public Exception? ResetPasswordException { get; set; }
    public ResetPasswordRequest? ResetPasswordRequest { get; private set; }

    public Task<RegisterOrganizationAccountResponse> RegisterOrganizationOwnerAsync(
        RegisterOrganizationAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (RegisterException is not null)
        {
            throw RegisterException;
        }

        return Task.FromResult(RegisterResponse);
    }

    public Task<LoginBusinessAccountResponse> LoginBusinessOwnerAsync(
        LoginBusinessAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (LoginBusinessException is not null)
        {
            throw LoginBusinessException;
        }

        return Task.FromResult(LoginBusinessResponse);
    }

    public Task ResetPasswordAsync(
        ResetPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ResetPasswordRequest = request;
        if (ResetPasswordException is not null)
        {
            throw ResetPasswordException;
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeResidentAuthService : IResidentAuthService
{
    public LoginResidentAccountResponse Response { get; set; } = new();
    public Exception? Exception { get; set; }

    public Task<LoginResidentAccountResponse> LoginAsync(
        LoginResidentAccountRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        return Task.FromResult(Response);
    }
}

internal sealed class FakeChangePasswordService : IChangePasswordService
{
    public Guid AccountId { get; private set; }
    public Exception? Exception { get; set; }

    public Task ChangePasswordAsync(
        Guid accountId,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        AccountId = accountId;
        if (Exception is not null)
        {
            throw Exception;
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeLogoutService : ILogoutService
{
    public LogoutRequest? Request { get; private set; }

    public Task LogoutAsync(
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        Request = request;
        return Task.CompletedTask;
    }
}

internal sealed class FakeOrganizationResidentService : IOrganizationResidentService
{
    public IReadOnlyList<OrganizationResidentDto>? Residents { get; set; } = [];
    public IReadOnlyList<OrganizationResidentInvitationDto>? Invitations { get; set; } = [];
    public ResidentMembershipResponse InviteResponse { get; set; } = new();
    public ResidentMembershipResponse AcceptResponse { get; set; } = new();
    public Exception? InviteException { get; set; }
    public Exception? AcceptException { get; set; }
    public Guid AcceptAccountId { get; private set; }
    public Guid AcceptMembershipId { get; private set; }
    public bool AcceptIsAccepted { get; private set; }

    public Task<IReadOnlyList<OrganizationResidentDto>?> GetResidentsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Residents);
    }

    public Task<IReadOnlyList<OrganizationResidentInvitationDto>?> GetResidentInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Invitations);
    }

    public Task<ResidentMembershipResponse> InviteResidentAsync(
        Guid organizationId,
        InviteResidentToOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (InviteException is not null)
        {
            throw InviteException;
        }

        return Task.FromResult(InviteResponse);
    }

    public Task<ResidentMembershipResponse> AcceptInvitationAsync(
        Guid accountId,
        Guid membershipId,
        bool isAccepted = true,
        CancellationToken cancellationToken = default)
    {
        AcceptAccountId = accountId;
        AcceptMembershipId = membershipId;
        AcceptIsAccepted = isAccepted;

        if (AcceptException is not null)
        {
            throw AcceptException;
        }

        return Task.FromResult(AcceptResponse);
    }
}

internal sealed class FakeResidentService : IResidentService
{
    public CreateResidentAccountResponse CreateResponse { get; set; } = new();
    public IReadOnlyList<ResidentDto> SearchResults { get; set; } = [];
    public IReadOnlyList<ResidentAccommodationDto> Accommodations { get; set; } = [];
    public Exception? CreateException { get; set; }
    public string? SearchPhone { get; private set; }

    public Task<CreateResidentAccountResponse> CreateResidentAccountAsync(
        CreateResidentAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (CreateException is not null)
        {
            throw CreateException;
        }

        return Task.FromResult(CreateResponse);
    }

    public Task<IReadOnlyList<ResidentDto>> SearchByPhoneAsync(
        string? phone,
        CancellationToken cancellationToken = default)
    {
        SearchPhone = phone;
        return Task.FromResult(SearchResults);
    }

    public Task<IReadOnlyList<ResidentAccommodationDto>> GetActiveAccommodationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Accommodations);
    }
}

internal sealed class FakeResidentInvitationService : IResidentInvitationService
{
    public IReadOnlyList<ResidentInvitationDto> Invitations { get; set; } = [];

    public Task<IReadOnlyList<ResidentInvitationDto>> GetPendingInvitationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Invitations);
    }
}

internal sealed class FakeRoomService : IRoomService
{
    public IReadOnlyList<RoomDto> Rooms { get; set; } = [];
    public RoomDto CreateResponse { get; set; } = new();
    public RoomDto UpdateResponse { get; set; } = new();
    public RoomImageDto UploadResponse { get; set; } = new();
    public RoomDto UpdatePriceResponse { get; set; } = new();
    public RoomDto UpdateOccupantsResponse { get; set; } = new();
    public RoomDto UpdateAmenitiesResponse { get; set; } = new();
    public ArgumentException? GetRoomsException { get; set; }
    public KeyNotFoundException? UpdateException { get; set; }
    public KeyNotFoundException? DeleteException { get; set; }
    public KeyNotFoundException? UploadException { get; set; }
    public ArgumentException? UploadArgumentException { get; set; }
    public KeyNotFoundException? UpdatePriceException { get; set; }
    public KeyNotFoundException? UpdateOccupantsException { get; set; }
    public KeyNotFoundException? UpdateAmenitiesException { get; set; }
    public Guid GetRoomsPropertyId { get; private set; }
    public string? GetRoomsSearch { get; private set; }
    public string? GetRoomsStatus { get; private set; }
    public Guid DeleteRoomId { get; private set; }

    public Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        GetRoomsPropertyId = propertyId;
        GetRoomsSearch = search;
        GetRoomsStatus = status;

        if (GetRoomsException is not null)
        {
            throw GetRoomsException;
        }

        return Task.FromResult(Rooms);
    }

    public Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateResponse);
    }

    public Task<RoomDto> UpdateRoomAsync(Guid roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        if (UpdateException is not null)
        {
            throw UpdateException;
        }

        return Task.FromResult(UpdateResponse);
    }

    public Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        DeleteRoomId = roomId;
        if (DeleteException is not null)
        {
            throw DeleteException;
        }

        return Task.CompletedTask;
    }

    public Task<RoomImageDto> UploadRoomImageAsync(
        Guid roomId,
        UploadRoomImageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (UploadException is not null)
        {
            throw UploadException;
        }

        if (UploadArgumentException is not null)
        {
            throw UploadArgumentException;
        }

        return Task.FromResult(UploadResponse);
    }

    public Task<RoomDto> UpdateBasePriceAsync(
        Guid roomId,
        UpdateBasePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (UpdatePriceException is not null)
        {
            throw UpdatePriceException;
        }

        return Task.FromResult(UpdatePriceResponse);
    }

    public Task<RoomDto> UpdateMaxOccupantsAsync(
        Guid roomId,
        UpdateMaxOccupantsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (UpdateOccupantsException is not null)
        {
            throw UpdateOccupantsException;
        }

        return Task.FromResult(UpdateOccupantsResponse);
    }

    public Task<RoomDto> UpdateAmenitiesAsync(
        Guid roomId,
        UpdateAmenitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (UpdateAmenitiesException is not null)
        {
            throw UpdateAmenitiesException;
        }

        return Task.FromResult(UpdateAmenitiesResponse);
    }

    public KeyNotFoundException? DeleteImageException { get; set; }

    public Task DeleteRoomImageAsync(
        Guid roomId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        if (DeleteImageException is not null)
        {
            throw DeleteImageException;
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakePropertyService : IPropertyService
{
    public IReadOnlyList<PropertyDto> Properties { get; set; } = [];
    public PropertyDto CreateResponse { get; set; } = new();
    public PropertyDto UpdateResponse { get; set; } = new();

    public Task<IReadOnlyList<PropertyDto>> GetPropertiesByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Properties);
    }

    public Task<PropertyDto> CreatePropertyAsync(Guid organizationId, CreatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateResponse);
    }

    public Task<PropertyDto> UpdatePropertyAsync(Guid organizationId, Guid propertyId, UpdatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(UpdateResponse);
    }

    public Task DeletePropertyAsync(Guid organizationId, Guid propertyId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

internal sealed class FakePropertyCatalogService : IPropertyCatalogService
{
    public Guid OrganizationId { get; private set; }
    public Guid PropertyId { get; private set; }
    public string? Search { get; private set; }
    public IReadOnlyList<PropertyServiceDto>? Services { get; set; }
    public CreatePropertyServiceRequest? CreateRequest { get; private set; }
    public PropertyServiceDto CreateResponse { get; set; } = new();
    public KeyNotFoundException? CreateNotFoundException { get; set; }
    public InvalidOperationException? CreateInvalidOperationException { get; set; }
    public ArgumentException? CreateArgumentException { get; set; }
    public Guid PropertyServiceId { get; private set; }
    public UpdatePropertyServiceRequest? UpdateRequest { get; private set; }
    public PropertyServiceDto UpdateResponse { get; set; } = new();
    public KeyNotFoundException? UpdateException { get; set; }
    public ArgumentException? UpdateArgumentException { get; set; }

    public Task<IReadOnlyList<PropertyServiceDto>?> GetPropertyServicesAsync(
        Guid organizationId,
        Guid propertyId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        OrganizationId = organizationId;
        PropertyId = propertyId;
        Search = search;
        return Task.FromResult(Services);
    }

    public Task<PropertyServiceDto> CreatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        CreatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        OrganizationId = organizationId;
        PropertyId = propertyId;
        CreateRequest = request;

        if (CreateNotFoundException is not null)
        {
            throw CreateNotFoundException;
        }

        if (CreateInvalidOperationException is not null)
        {
            throw CreateInvalidOperationException;
        }

        if (CreateArgumentException is not null)
        {
            throw CreateArgumentException;
        }

        return Task.FromResult(CreateResponse);
    }

    public Task<PropertyServiceDto> UpdatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        Guid propertyServiceId,
        UpdatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        OrganizationId = organizationId;
        PropertyId = propertyId;
        PropertyServiceId = propertyServiceId;
        UpdateRequest = request;

        if (UpdateException is not null)
        {
            throw UpdateException;
        }

        if (UpdateArgumentException is not null)
        {
            throw UpdateArgumentException;
        }

        return Task.FromResult(UpdateResponse);
    }
}

internal sealed class FakeSampleDataService : ISampleDataService
{
    public IReadOnlyList<OrganizationDto> Organizations { get; set; } = [];
    public int Take { get; private set; }

    public Task<IReadOnlyList<OrganizationDto>> GetOrganizationsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        Take = take;
        return Task.FromResult(Organizations);
    }
}
