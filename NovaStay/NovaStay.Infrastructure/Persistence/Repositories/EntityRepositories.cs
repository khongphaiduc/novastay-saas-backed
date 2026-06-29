using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Common;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Mapping;
using DomainOrganization = NovaStay.Domain.Entities.OrganizationEntity;
using DatabaseOrganization = NovaStay.Infrastructure.Models.Organization;
using DomainAsset = NovaStay.Domain.Entities.AssetEntity;
using DatabaseAsset = NovaStay.Infrastructure.Models.Asset;
using DomainAssetAssignment = NovaStay.Domain.Entities.AssetAssignmentEntity;
using DatabaseAssetAssignment = NovaStay.Infrastructure.Models.AssetAssignment;
using DomainBooking = NovaStay.Domain.Entities.BookingEntity;
using DatabaseBooking = NovaStay.Infrastructure.Models.Booking;
using DomainBroker = NovaStay.Domain.Entities.BrokerEntity;
using DatabaseBroker = NovaStay.Infrastructure.Models.Broker;
using DomainContract = NovaStay.Domain.Entities.ContractEntity;
using DatabaseContract = NovaStay.Infrastructure.Models.Contract;
using DomainInvoice = NovaStay.Domain.Entities.InvoiceEntity;
using DatabaseInvoice = NovaStay.Infrastructure.Models.Invoice;
using DomainMaintenanceTicket = NovaStay.Domain.Entities.MaintenanceTicketEntity;
using DatabaseMaintenanceTicket = NovaStay.Infrastructure.Models.MaintenanceTicket;
using DomainPermission = NovaStay.Domain.Entities.PermissionEntity;
using DatabasePermission = NovaStay.Infrastructure.Models.Permission;
using DomainProperty = NovaStay.Domain.Entities.PropertyEntity;
using DatabaseProperty = NovaStay.Infrastructure.Models.Property;
using DomainResident = NovaStay.Domain.Entities.ResidentEntity;
using DatabaseResident = NovaStay.Infrastructure.Models.Resident;
using DomainResidentMembership = NovaStay.Domain.Entities.ResidentMembershipEntity;
using DatabaseResidentMembership = NovaStay.Infrastructure.Models.ResidentMembership;
using DomainRole = NovaStay.Domain.Entities.RoleEntity;
using DatabaseRole = NovaStay.Infrastructure.Models.Role;
using DomainRoom = NovaStay.Domain.Entities.RoomEntity;
using DatabaseRoom = NovaStay.Infrastructure.Models.Room;
using DomainRoomAvailability = NovaStay.Domain.Entities.RoomAvailabilityEntity;
using DatabaseRoomAvailability = NovaStay.Infrastructure.Models.RoomAvailability;
using DomainRoomImage = NovaStay.Domain.Entities.RoomImageEntity;
using DatabaseRoomImage = NovaStay.Infrastructure.Models.RoomImage;
using DomainSubscriptionPackage = NovaStay.Domain.Entities.SubscriptionPackageEntity;
using DatabaseSubscriptionPackage = NovaStay.Infrastructure.Models.SubscriptionPackage;
using DomainTechnician = NovaStay.Domain.Entities.TechnicianEntity;
using DatabaseTechnician = NovaStay.Infrastructure.Models.Technician;
using DomainAccount = NovaStay.Domain.Entities.AccountEntity;
using DatabaseAccount = NovaStay.Infrastructure.Models.Account;
using DomainAccountRefreshToken = NovaStay.Domain.Entities.AccountRefreshTokenEntity;
using DatabaseAccountRefreshToken = NovaStay.Infrastructure.Models.AccountRefreshToken;
using DomainStaffUser = NovaStay.Domain.Entities.StaffUserEntity;
using DatabaseStaffUser = NovaStay.Infrastructure.Models.StaffUser;
using Microsoft.EntityFrameworkCore;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal sealed class AssetRepository : Repository<DomainAsset, DatabaseAsset>, IAssetRepository
{
    private readonly HostContext _context;

    public AssetRepository(HostContext context, IDatabaseModelMapper<DomainAsset, DatabaseAsset> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    /// <summary>TASK-055: Lấy danh sách tài sản kèm assignment mới nhất, có thể lọc theo search/category/status</summary>
    public async Task<IReadOnlyList<AssetDto>> GetAssetsWithCurrentStatusAsync(
        Guid organizationId,
        string? search = null,
        string? category = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        // Subquery lấy ID assignment mới nhất của mỗi tài sản
        var latestAssignmentIds = _context.AssetAssignments
            .GroupBy(a => a.AssetId)
            .Select(g => g.OrderByDescending(a => a.AssignedAt).First().Id);

        var query = from asset in _context.Assets
                    where asset.OrganizationId == organizationId && !asset.IsDeleted
                    join aa in _context.AssetAssignments.Where(a => latestAssignmentIds.Contains(a.Id))
                        on asset.Id equals aa.AssetId into assignments
                    from latestAss in assignments.DefaultIfEmpty()
                    join room in _context.Rooms
                        on latestAss.RoomId equals room.Id into rooms
                    from assignedRoom in rooms.DefaultIfEmpty()
                    select new AssetDto
                    {
                        Id = asset.Id,
                        OrganizationId = asset.OrganizationId,
                        AssetName = asset.AssetName,
                        Category = asset.Category,
                        Brand = asset.Brand,
                        Model = asset.Model,
                        AssetCode = asset.AssetCode,
                        PurchaseDate = asset.PurchaseDate,
                        WarrantyExpiryDate = asset.WarrantyExpiryDate,
                        BaseValue = asset.BaseValue,
                        CreatedAt = asset.CreatedAt,
                        IsDeleted = asset.IsDeleted,
                        CurrentAssignmentId = latestAss != null ? latestAss.Id : (Guid?)null,
                        CurrentRoomId = latestAss != null ? latestAss.RoomId : null,
                        CurrentRoomNumber = assignedRoom != null ? assignedRoom.RoomNumber : null,
                        CurrentStatus = latestAss != null ? latestAss.Status : "Good",
                        CurrentNote = latestAss != null ? latestAss.Note : null,
                        LastAssignedAt = latestAss != null ? latestAss.AssignedAt : null
                    };

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                a.AssetName.Contains(search) ||
                (a.AssetCode != null && a.AssetCode.Contains(search)) ||
                (a.Brand != null && a.Brand.Contains(search)));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.CurrentStatus == status);

        return await query
            .OrderBy(a => a.AssetName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Task phát sinh: Lấy tài sản trong phòng</summary>
    public async Task<IReadOnlyList<AssetDto>> GetAssetsByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var latestAssignmentIds = _context.AssetAssignments
            .GroupBy(a => a.AssetId)
            .Select(g => g.OrderByDescending(a => a.AssignedAt).First().Id);

        var query = from asset in _context.Assets
                    where !asset.IsDeleted
                    join aa in _context.AssetAssignments
                        .Where(a => latestAssignmentIds.Contains(a.Id) && a.RoomId == roomId)
                        on asset.Id equals aa.AssetId
                    join room in _context.Rooms on aa.RoomId equals room.Id into rooms
                    from assignedRoom in rooms.DefaultIfEmpty()
                    select new AssetDto
                    {
                        Id = asset.Id,
                        OrganizationId = asset.OrganizationId,
                        AssetName = asset.AssetName,
                        Category = asset.Category,
                        Brand = asset.Brand,
                        Model = asset.Model,
                        AssetCode = asset.AssetCode,
                        PurchaseDate = asset.PurchaseDate,
                        WarrantyExpiryDate = asset.WarrantyExpiryDate,
                        BaseValue = asset.BaseValue,
                        CreatedAt = asset.CreatedAt,
                        IsDeleted = asset.IsDeleted,
                        CurrentAssignmentId = aa.Id,
                        CurrentRoomId = aa.RoomId,
                        CurrentRoomNumber = assignedRoom != null ? assignedRoom.RoomNumber : null,
                        CurrentStatus = aa.Status,
                        CurrentNote = aa.Note,
                        LastAssignedAt = aa.AssignedAt
                    };

        return await query.OrderBy(a => a.AssetName).ToListAsync(cancellationToken);
    }

    /// <summary>Task phát sinh: Thống kê tài sản theo Organization</summary>
    public async Task<AssetStatisticsDto> GetStatisticsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var latestAssignmentIds = _context.AssetAssignments
            .GroupBy(a => a.AssetId)
            .Select(g => g.OrderByDescending(a => a.AssignedAt).First().Id);

        var stats = await (
            from asset in _context.Assets
            where asset.OrganizationId == organizationId && !asset.IsDeleted
            join aa in _context.AssetAssignments.Where(a => latestAssignmentIds.Contains(a.Id))
                on asset.Id equals aa.AssetId into assignments
            from latestAss in assignments.DefaultIfEmpty()
            select new { latestAss }
        ).ToListAsync(cancellationToken);

        return new AssetStatisticsDto
        {
            TotalAssets = stats.Count,
            InStorage = stats.Count(x => x.latestAss == null || x.latestAss.RoomId == null),
            InUse = stats.Count(x => x.latestAss != null && x.latestAss.RoomId != null
                && (x.latestAss.Status == "Good" || x.latestAss.Status == "Working")),
            Damaged = stats.Count(x => x.latestAss != null && x.latestAss.Status == "Damaged"),
            InMaintenance = stats.Count(x => x.latestAss != null && x.latestAss.Status == "Maintenance")
        };
    }
}

internal sealed class AssetAssignmentRepository : Repository<DomainAssetAssignment, DatabaseAssetAssignment>, IAssetAssignmentRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainAssetAssignment, DatabaseAssetAssignment> _assetAssMapper;

    public AssetAssignmentRepository(HostContext context, IDatabaseModelMapper<DomainAssetAssignment, DatabaseAssetAssignment> mapper)
        : base(context, mapper)
    {
        _context = context;
        _assetAssMapper = mapper;
    }

    /// <summary>TASK-062: Lịch sử luân chuyển của một tài sản, kèm số phòng</summary>
    public async Task<IReadOnlyList<AssetHistoryDto>> GetHistoryByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var history = await (
            from aa in _context.AssetAssignments
            where aa.AssetId == assetId
            join room in _context.Rooms on aa.RoomId equals room.Id into rooms
            from assignedRoom in rooms.DefaultIfEmpty()
            orderby aa.AssignedAt descending
            select new AssetHistoryDto
            {
                Id = aa.Id,
                AssetId = aa.AssetId,
                RoomId = aa.RoomId,
                RoomNumber = assignedRoom != null ? assignedRoom.RoomNumber : null,
                Status = aa.Status,
                Note = aa.Note,
                AssignedAt = aa.AssignedAt
            }
        ).ToListAsync(cancellationToken);

        return history;
    }

    /// <summary>Lấy bản ghi assignment mới nhất của tài sản</summary>
    public async Task<DomainAssetAssignment?> GetLatestByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var latest = await _context.AssetAssignments
            .AsNoTracking()
            .Where(aa => aa.AssetId == assetId)
            .OrderByDescending(aa => aa.AssignedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return latest is null ? null : _assetAssMapper.ToDomain(latest);
    }
}

internal sealed class BookingRepository : Repository<DomainBooking, DatabaseBooking>, IBookingRepository
{
    public BookingRepository(HostContext context, IDatabaseModelMapper<DomainBooking, DatabaseBooking> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class BrokerRepository : Repository<DomainBroker, DatabaseBroker>, IBrokerRepository
{
    public BrokerRepository(HostContext context, IDatabaseModelMapper<DomainBroker, DatabaseBroker> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class ContractRepository : Repository<DomainContract, DatabaseContract>, IContractRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainContract, DatabaseContract> _mapper;

    public ContractRepository(HostContext context, IDatabaseModelMapper<DomainContract, DatabaseContract> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    private static ContractDetailDto MapToDetail(DatabaseContract c) => new()
    {
        Id = c.Id,
        OrganizationId = c.OrganizationId,
        PropertyId = c.PropertyId,
        RoomId = c.RoomId,
        ResidentId = c.ResidentId,
        BrokerId = c.BrokerId,
        BookingId = c.BookingId,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        DepositAmount = c.DepositAmount,
        BrokerCommission = c.BrokerCommission,
        CommissionStatus = c.CommissionStatus,
        ContractPdfUrl = c.ContractPdfUrl,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        ResidentName = c.Resident != null ? c.Resident.FullName : null,
        ResidentPhone = c.Resident != null ? c.Resident.Phone : null,
        RoomNumber = c.Room != null ? c.Room.RoomNumber : null,
        BasePrice = c.Room != null ? c.Room.BasePrice : 0
    };

    public async Task<IReadOnlyList<ContractDetailDto>> GetContractsWithDetailsAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        Guid? residentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .AsNoTracking()
            .Include(c => c.Resident)
            .Include(c => c.Room)
            .Where(c => c.OrganizationId == organizationId);

        if (residentId.HasValue)
            query = query.Where(c => c.ResidentId == residentId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.Trim().ToLower();
            query = query.Where(c =>
                c.Resident.FullName.ToLower().Contains(lower) ||
                c.Room.RoomNumber.ToLower().Contains(lower));
        }

        var contracts = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
        return contracts.Select(MapToDetail).ToList();
    }

    public async Task<ContractDetailDto?> GetContractDetailByIdAsync(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var c = await _context.Contracts
            .AsNoTracking()
            .Include(x => x.Resident)
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken);

        return c is null ? null : MapToDetail(c);
    }

    public async Task<IReadOnlyList<ContractDetailDto>> GetExpiringSoonAsync(
        Guid organizationId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var contracts = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Resident)
            .Include(c => c.Room)
            .Where(c => c.OrganizationId == organizationId
                && c.Status == "Active"
                && c.EndDate >= today
                && c.EndDate <= threshold)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);

        return contracts.Select(MapToDetail).ToList();
    }

    public async Task<IReadOnlyList<DomainContract>> GetAllActiveExpiringOnDateAsync(
        DateTime targetDate,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = DateOnly.FromDateTime(targetDate);

        var databaseContracts = await _context.Contracts
            .AsNoTracking()
            .Where(c => c.Status == "Active" && c.EndDate == dateOnly)
            .ToListAsync(cancellationToken);

        // Map database entities to domain entities using the injected mapper
        return databaseContracts.Select(_mapper.ToDomain).ToList();
    }
}


internal sealed class InvoiceRepository : Repository<DomainInvoice, DatabaseInvoice>, IInvoiceRepository
{
    public InvoiceRepository(HostContext context, IDatabaseModelMapper<DomainInvoice, DatabaseInvoice> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class MaintenanceTicketRepository : Repository<DomainMaintenanceTicket, DatabaseMaintenanceTicket>, IMaintenanceTicketRepository
{
    private readonly HostContext _context;

    public MaintenanceTicketRepository(HostContext context, IDatabaseModelMapper<DomainMaintenanceTicket, DatabaseMaintenanceTicket> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    private static MaintenanceTicketDetailDto MapToDetail(DatabaseMaintenanceTicket t) => new()
    {
        Id = t.Id,
        OrganizationId = t.OrganizationId,
        RoomId = t.RoomId,
        ResidentId = t.ResidentId,
        Category = t.Category,
        UserDescription = t.UserDescription,
        IncidentImageUrl = t.IncidentImageUrl,
        Status = t.Status,
        TechnicianId = t.TechnicianId,
        ResolvedImageUrl = t.ResolvedImageUrl,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        ResidentName = t.Resident != null ? t.Resident.FullName : null,
        RoomNumber = t.Room != null ? t.Room.RoomNumber : null
    };

    public async Task<(IReadOnlyList<MaintenanceTicketDetailDto> Items, int TotalCount)> GetByRoomIdAsync(
        Guid roomId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenanceTickets
            .AsNoTracking()
            .Where(t => t.RoomId == roomId);

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .Include(t => t.Resident)
            .Include(t => t.Room)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (tickets.Select(MapToDetail).ToList(), totalCount);
    }

    public async Task<IReadOnlyList<MaintenanceTicketDetailDto>> GetByOrganizationIdAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenanceTickets
            .AsNoTracking()
            .Include(t => t.Resident)
            .Include(t => t.Room)
            .Where(t => t.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status.Trim());

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
        return tickets.Select(MapToDetail).ToList();
    }
}


internal sealed class PermissionRepository : Repository<DomainPermission, DatabasePermission>, IPermissionRepository
{
    public PermissionRepository(HostContext context, IDatabaseModelMapper<DomainPermission, DatabasePermission> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class PropertyRepository : Repository<DomainProperty, DatabaseProperty>, IPropertyRepository
{
    public PropertyRepository(HostContext context, IDatabaseModelMapper<DomainProperty, DatabaseProperty> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class PropertyServiceRepository : IPropertyServiceRepository
{
    private readonly HostContext _context;

    public PropertyServiceRepository(HostContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PropertyServiceDto>?> GetPropertyServicesAsync(
        Guid organizationId,
        Guid propertyId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == propertyId && entity.OrganizationId == organizationId,
                cancellationToken);

        if (!propertyExists)
        {
            return null;
        }

        var query = _context.PropertyServices
            .AsNoTracking()
            .Where(service => service.PropertyId == propertyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(service => service.ServiceName.Contains(search));
        }

        return await query
            .OrderBy(service => service.ServiceName)
            .Select(service => new PropertyServiceDto
            {
                Id = service.Id,
                PropertyId = service.PropertyId,
                ServiceName = service.ServiceName,
                ServiceCode = service.ServiceCode,
                Description = service.Description,
                DefaultPrice = service.DefaultPrice,
                Unit = service.Unit,
                BillingCycle = service.BillingCycle,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PropertyServiceDto> CreatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        CreatePropertyServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var propertyExists = await _context.Properties
            .AsNoTracking()
            .AnyAsync(
                entity => entity.Id == propertyId && entity.OrganizationId == organizationId,
                cancellationToken);

        if (!propertyExists)
        {
            throw new KeyNotFoundException("Property not found in organization.");
        }

        var duplicatedName = await _context.PropertyServices
            .AsNoTracking()
            .AnyAsync(
                service => service.PropertyId == propertyId && service.ServiceName == request.ServiceName,
                cancellationToken);

        if (duplicatedName)
        {
            throw new InvalidOperationException("Property service name already exists in property.");
        }

        var propertyService = new Models.PropertyService
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            ServiceName = request.ServiceName,
            ServiceCode = request.ServiceCode,
            Description = request.Description,
            DefaultPrice = request.DefaultPrice,
            Unit = request.Unit,
            BillingCycle = request.BillingCycle,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.PropertyServices.AddAsync(propertyService, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new PropertyServiceDto
        {
            Id = propertyService.Id,
            PropertyId = propertyService.PropertyId,
            ServiceName = propertyService.ServiceName,
            ServiceCode = propertyService.ServiceCode,
            Description = propertyService.Description,
            DefaultPrice = propertyService.DefaultPrice,
            Unit = propertyService.Unit,
            BillingCycle = propertyService.BillingCycle,
            IsActive = propertyService.IsActive,
            CreatedAt = propertyService.CreatedAt,
            UpdatedAt = propertyService.UpdatedAt
        };
    }

    public async Task<PropertyServiceDto> UpdatePropertyServiceAsync(
        Guid organizationId,
        Guid propertyId,
        Guid propertyServiceId,
        decimal defaultPrice,
        string? billingCycle,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var propertyService = await _context.PropertyServices
            .FirstOrDefaultAsync(
                service => service.Id == propertyServiceId &&
                           service.PropertyId == propertyId &&
                           service.Property.OrganizationId == organizationId,
                cancellationToken);

        if (propertyService is null)
        {
            throw new KeyNotFoundException("Property service not found in property.");
        }

        propertyService.DefaultPrice = defaultPrice;
        propertyService.BillingCycle = string.IsNullOrWhiteSpace(billingCycle) ? null : billingCycle.Trim();
        propertyService.IsActive = isActive;
        propertyService.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new PropertyServiceDto
        {
            Id = propertyService.Id,
            PropertyId = propertyService.PropertyId,
            ServiceName = propertyService.ServiceName,
            ServiceCode = propertyService.ServiceCode,
            Description = propertyService.Description,
            DefaultPrice = propertyService.DefaultPrice,
            Unit = propertyService.Unit,
            BillingCycle = propertyService.BillingCycle,
            IsActive = propertyService.IsActive,
            CreatedAt = propertyService.CreatedAt,
            UpdatedAt = propertyService.UpdatedAt
        };
    }
}

internal sealed class ResidentRepository : Repository<DomainResident, DatabaseResident>, IResidentRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainResident, DatabaseResident> _mapper;

    public ResidentRepository(HostContext context, IDatabaseModelMapper<DomainResident, DatabaseResident> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainResident?> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.AccountId == accountId,
                cancellationToken);

        return resident is null ? null : _mapper.ToDomain(resident);
    }

    public async Task<DomainResident?> GetByIdentityCardNumberAsync(
        string identityCardNumber,
        CancellationToken cancellationToken = default)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.IdentityCardNumber == identityCardNumber,
                cancellationToken);

        return resident is null ? null : _mapper.ToDomain(resident);
    }

    public async Task<IReadOnlyList<DomainResident>> SearchByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        var residents = await _context.Residents
            .AsNoTracking()
            .Where(resident => resident.Phone.Contains(phone))
            .OrderBy(resident => resident.FullName)
            .ToListAsync(cancellationToken);

        return residents.Select(_mapper.ToDomain).ToList();
    }
}

internal sealed class ResidentMembershipRepository : Repository<DomainResidentMembership, DatabaseResidentMembership>, IResidentMembershipRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainResidentMembership, DatabaseResidentMembership> _mapper;

    public ResidentMembershipRepository(HostContext context, IDatabaseModelMapper<DomainResidentMembership, DatabaseResidentMembership> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainResidentMembership?> GetActiveByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .Where(entity => entity.AccountId == accountId
                && (entity.Status == ResidentMembershipStatuses.Active
                    || entity.Status == ResidentMembershipStatuses.LegacyActive))
            .OrderByDescending(entity => entity.ActivatedAt ?? entity.JoinedAt ?? entity.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public async Task<DomainResidentMembership?> GetByResidentAndOrganizationAsync(
        Guid residentId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.ResidentId == residentId && entity.OrganizationId == organizationId,
                cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public async Task<DomainResidentMembership?> GetByIdForAccountAsync(
        Guid membershipId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _context.ResidentMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.Id == membershipId && entity.AccountId == accountId,
                cancellationToken);

        return membership is null ? null : _mapper.ToDomain(membership);
    }

    public Task<bool> ExistsByMembershipCodeAsync(
        string membershipCode,
        CancellationToken cancellationToken = default)
    {
        return _context.ResidentMemberships
            .AsNoTracking()
            .AnyAsync(
                entity => entity.MembershipCode == membershipCode,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<OrganizationResidentDto> Data, int TotalRecords)> GetResidentsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        var query = _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId);

        if (normalizedStatus is not null)
        {
            query = query.Where(membership => membership.Status == normalizedStatus);
        }
        else
        {
            query = query.Where(membership => membership.Status != "INACTIVE" && membership.Status != "REMOVED" && membership.Status == "ACTIVE");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(membership =>
                membership.Resident.FullName.ToLower().Contains(searchLower) ||
                membership.Resident.Phone.Contains(searchLower));
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderBy(membership => membership.Status)
            .ThenBy(membership => membership.Resident.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(membership => new OrganizationResidentDto
            {
                MembershipId = membership.Id,
                OrganizationId = membership.OrganizationId,
                ResidentId = membership.ResidentId,
                AccountId = membership.AccountId,
                MembershipCode = membership.MembershipCode,
                MembershipStatus = membership.Status,
                InvitedAt = membership.InvitedAt,
                RespondedAt = membership.RespondedAt,
                ActivatedAt = membership.ActivatedAt,
                JoinedAt = membership.JoinedAt,
                FullName = membership.Resident.FullName,
                Phone = membership.Resident.Phone,
                Email = membership.Resident.Email,
                IdentityCardNumber = membership.Resident.IdentityCardNumber,
                ProfileImageUrl = membership.Resident.ProfileImageUrl,
                AccountIsActive = membership.Account.IsActive,
                MustSetPassword = membership.Account.MustSetPassword
            })
            .ToListAsync(cancellationToken);
        return (data, totalRecords);
    }

    public async Task<IReadOnlyList<OrganizationResidentInvitationDto>> GetInvitationsByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        var query = _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId);

        if (normalizedStatus is not null)
        {
            query = query.Where(membership => membership.Status == normalizedStatus);
        }

        return await query
            .OrderByDescending(membership => membership.InvitedAt ?? membership.CreatedAt)
            .ThenBy(membership => membership.Resident.FullName)
            .Select(membership => new OrganizationResidentInvitationDto
            {
                MembershipId = membership.Id,
                OrganizationId = membership.OrganizationId,
                ResidentId = membership.ResidentId,
                AccountId = membership.AccountId,
                MembershipCode = membership.MembershipCode,
                Status = membership.Status,
                InvitedAt = membership.InvitedAt,
                RespondedAt = membership.RespondedAt,
                JoinedAt = membership.JoinedAt,
                ActivatedAt = membership.ActivatedAt,
                ResidentName = membership.Resident.FullName,
                ResidentPhone = membership.Resident.Phone,
                ResidentEmail = "",
                IdentityCardNumber = ""
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResidentAccommodationDto>> GetActiveAccommodationsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.AccountId == accountId
                && (membership.Status == ResidentMembershipStatuses.Active
                    || membership.Status == ResidentMembershipStatuses.LegacyActive))
            .OrderByDescending(membership => membership.ActivatedAt ?? membership.JoinedAt ?? membership.CreatedAt)
            .ThenBy(membership => membership.Organization.BusinessName)
            .Select(membership => new ResidentAccommodationDto
            {
                MembershipId = membership.Id,
                AccountId = membership.AccountId,
                ResidentId = membership.ResidentId,
                OrganizationId = membership.OrganizationId,
                MembershipCode = membership.MembershipCode,
                MembershipStatus = membership.Status,
                JoinedAt = membership.JoinedAt,
                ActivatedAt = membership.ActivatedAt,
                BusinessName = membership.Organization.BusinessName,
                BusinessArea = membership.Organization.BusinessArea,
                OwnerPhone = membership.Organization.OwnerPhone,
                OwnerEmail = membership.Organization.OwnerEmail
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResidentInvitationDto>> GetPendingInvitationsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResidentMemberships
            .AsNoTracking()
            .Where(membership => membership.AccountId == accountId
                && membership.Status == ResidentMembershipStatuses.Pending)
            .OrderByDescending(membership => membership.InvitedAt ?? membership.CreatedAt)
            .ThenBy(membership => membership.Organization.BusinessName)
            .Select(membership => new ResidentInvitationDto
            {
                MembershipId = membership.Id,
                AccountId = membership.AccountId,
                ResidentId = membership.ResidentId,
                OrganizationId = membership.OrganizationId,
                MembershipCode = membership.MembershipCode,
                Status = membership.Status,
                InvitedAt = membership.InvitedAt,
                RespondedAt = membership.RespondedAt,
                JoinedAt = membership.JoinedAt,
                ActivatedAt = membership.ActivatedAt,
                CreatedAt = membership.CreatedAt,
                BusinessName = membership.Organization.BusinessName,
                BusinessArea = membership.Organization.BusinessArea,
                OwnerPhone = membership.Organization.OwnerPhone,
                OwnerEmail = membership.Organization.OwnerEmail
            })
            .ToListAsync(cancellationToken);
    }
}

internal sealed class RoleRepository : Repository<DomainRole, DatabaseRole>, IRoleRepository
{
    public RoleRepository(HostContext context, IDatabaseModelMapper<DomainRole, DatabaseRole> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class RoomRepository : Repository<DomainRoom, DatabaseRoom>, IRoomRepository
{
    private readonly HostContext _context;

    public RoomRepository(HostContext context, IDatabaseModelMapper<DomainRoom, DatabaseRoom> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsWithImagesAsync(
        Guid propertyId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Where(r => r.PropertyId == propertyId && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.Trim().ToLower();
            query = query.Where(r => r.RoomNumber.ToLower().Contains(lower));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status.Trim());
        }

        return await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                MaxOccupants = r.MaxOccupants,
                AmenitiesJson = r.AmenitiesJson,
                RowVersion = r.RowVersion,
                CreatedAt = r.CreatedAt,
                IsDeleted = r.IsDeleted,
                Images = r.RoomImages
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.UploadedAt)
                    .Select(i => new RoomImageDto
                    {
                        Id = i.Id,
                        RoomId = i.RoomId,
                        ImageUrl = i.ImageUrl,
                        IsCover = i.IsCover,
                        UploadedAt = i.UploadedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomDto?> GetRoomWithImagesAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(r => r.Id == roomId && !r.IsDeleted)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                MaxOccupants = r.MaxOccupants,
                AmenitiesJson = r.AmenitiesJson,
                RowVersion = r.RowVersion,
                CreatedAt = r.CreatedAt,
                IsDeleted = r.IsDeleted,
                Images = r.RoomImages
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.UploadedAt)
                    .Select(i => new RoomImageDto
                    {
                        Id = i.Id,
                        RoomId = i.RoomId,
                        ImageUrl = i.ImageUrl,
                        IsCover = i.IsCover,
                        UploadedAt = i.UploadedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

internal sealed class RoomAvailabilityRepository : Repository<DomainRoomAvailability, DatabaseRoomAvailability>, IRoomAvailabilityRepository
{
    public RoomAvailabilityRepository(HostContext context, IDatabaseModelMapper<DomainRoomAvailability, DatabaseRoomAvailability> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class RoomImageRepository : Repository<DomainRoomImage, DatabaseRoomImage>, IRoomImageRepository
{
    public RoomImageRepository(HostContext context, IDatabaseModelMapper<DomainRoomImage, DatabaseRoomImage> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class SubscriptionPackageRepository : Repository<DomainSubscriptionPackage, DatabaseSubscriptionPackage>, ISubscriptionPackageRepository
{
    public SubscriptionPackageRepository(HostContext context, IDatabaseModelMapper<DomainSubscriptionPackage, DatabaseSubscriptionPackage> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class TechnicianRepository : Repository<DomainTechnician, DatabaseTechnician>, ITechnicianRepository
{
    public TechnicianRepository(HostContext context, IDatabaseModelMapper<DomainTechnician, DatabaseTechnician> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class StaffUserRepository : Repository<DomainStaffUser, DatabaseStaffUser>, IStaffUserRepository
{
    public StaffUserRepository(HostContext context, IDatabaseModelMapper<DomainStaffUser, DatabaseStaffUser> mapper)
        : base(context, mapper)
    {
    }
}

internal sealed class AccountRepository : Repository<DomainAccount, DatabaseAccount>, IAccountRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainAccount, DatabaseAccount> _mapper;

    public AccountRepository(HostContext context, IDatabaseModelMapper<DomainAccount, DatabaseAccount> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainAccount?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Phone == phone, cancellationToken);

        return account is null ? null : _mapper.ToDomain(account);
    }

    public async Task<DomainAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Email == email, cancellationToken);

        return account is null ? null : _mapper.ToDomain(account);
    }
}

internal sealed class AccountRefreshTokenRepository : Repository<DomainAccountRefreshToken, DatabaseAccountRefreshToken>, IAccountRefreshTokenRepository
{
    private readonly HostContext _context;

    public AccountRefreshTokenRepository(HostContext context, IDatabaseModelMapper<DomainAccountRefreshToken, DatabaseAccountRefreshToken> mapper)
        : base(context, mapper)
    {
        _context = context;
    }

    public async Task<bool> RevokeByTokenHashAsync(
        string tokenHash,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.AccountRefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash && token.RevokedAt == null,
                cancellationToken);

        if (refreshToken is null)
        {
            return false;
        }

        refreshToken.RevokedAt = revokedAt;
        refreshToken.RevokedByIp = revokedByIp;

        return true;
    }

    public async Task<int> RevokeActiveByAccountIdAsync(
        Guid accountId,
        DateTime revokedAt,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _context.AccountRefreshTokens
            .Where(token => token.AccountId == accountId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = revokedAt;
            refreshToken.RevokedByIp = revokedByIp;
        }

        return refreshTokens.Count;
    }
}

internal sealed class OrganizationRepository : Repository<DomainOrganization, DatabaseOrganization>, IOrganizationRepository
{
    private readonly HostContext _context;
    private readonly IDatabaseModelMapper<DomainOrganization, DatabaseOrganization> _mapper;

    public OrganizationRepository(HostContext context, IDatabaseModelMapper<DomainOrganization, DatabaseOrganization> mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DomainOrganization?> GetByOwnerEmailAsync(string ownerEmail, CancellationToken cancellationToken = default)
    {
        var Organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.OwnerEmail == ownerEmail, cancellationToken);

        return Organization is null ? null : _mapper.ToDomain(Organization);
    }
}
