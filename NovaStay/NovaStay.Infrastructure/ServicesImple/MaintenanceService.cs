using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class MaintenanceService : IMaintenanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MaintenanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // TASK-021: Xem lịch sử bảo trì của phòng
    public async Task<PagedResult<MaintenanceTicketDetailDto>> GetByRoomAsync(
        Guid roomId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (roomId == Guid.Empty) throw new ArgumentException("RoomId is required.");

        var (pagedItems, totalCount) = await _unitOfWork.MaintenanceTickets.GetByRoomIdAsync(roomId, pageIndex, pageSize, cancellationToken);

        return new PagedResult<MaintenanceTicketDetailDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    // TASK-022: Đánh dấu phòng bảo trì / trống & tạo phiếu nếu là bảo trì
    public async Task<string> MarkRoomStatusAsync(
        Guid roomId,
        string newStatus,
        Guid organizationId,
        Guid? residentId,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {roomId} not found.");

        // FIX: Ensure organizationId is valid by taking it from the room's property if possible
        var property = await _unitOfWork.Properties.GetByIdAsync(room.PropertyId, cancellationToken);
        if (property != null)
        {
            organizationId = property.OrganizationId;
        }

        var previousStatus = room.Status.ToString();

        if (newStatus == "Available")
        {
            var activeContracts = await _unitOfWork.Contracts.FindAsync(
                c => c.RoomId == roomId && c.Status == "Active", cancellationToken);
            if (activeContracts.Any())
            {
                newStatus = "Occupied";
            }
        }

        room.Status = new Status(newStatus);
        _unitOfWork.Rooms.Update(room);

        // Nếu chuyển sang Maintenance, tạo phiếu bảo trì tự động
        if (newStatus == "Maintenance")
        {
            var ticket = new MaintenanceTicketEntity
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                RoomId = roomId,
                ResidentId = residentId,
                Category = "Maintenance",
                UserDescription = description ?? "Phòng được đánh dấu bảo trì bởi chủ nhà.",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.MaintenanceTickets.AddAsync(ticket, cancellationToken);
        }
        else if (previousStatus == "Maintenance" && (newStatus == "Available" || newStatus == "Occupied"))
        {
            // Resolve the pending ticket
            var pendingTickets = await _unitOfWork.MaintenanceTickets.FindAsync(
                t => t.RoomId == roomId && t.Status == "Pending", cancellationToken);
            
            var ticket = pendingTickets.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
            if (ticket != null)
            {
                ticket.Status = "Resolved";
                ticket.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    ticket.UserDescription += $"\n[Lý do hoàn tất]: {description}";
                }
                _unitOfWork.MaintenanceTickets.Update(ticket);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return newStatus;
    }

    // TASK-021: Tạo phiếu bảo trì thủ công
    public async Task<MaintenanceTicketDetailDto> CreateTicketAsync(
        CreateMaintenanceTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.RoomId == Guid.Empty) throw new ArgumentException("RoomId is required.");
        if (string.IsNullOrWhiteSpace(request.Category)) throw new ArgumentException("Category is required.");

        var ticket = new MaintenanceTicketEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            RoomId = request.RoomId,
            ResidentId = request.ResidentId,
            Category = request.Category,
            UserDescription = request.UserDescription,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MaintenanceTickets.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MaintenanceTicketDetailDto>(ticket);
    }
}
