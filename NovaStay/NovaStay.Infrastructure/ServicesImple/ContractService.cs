using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

using MassTransit;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class ContractService : IContractService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public ContractService(IUnitOfWork unitOfWork, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    // TASK-036: Xem danh sách hợp đồng + TASK-037: Tìm kiếm
    public async Task<PagedResult<ContractDetailDto>> GetContractsAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        Guid? residentId = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var allContracts = await _unitOfWork.Contracts.GetContractsWithDetailsAsync(organizationId, search, status, residentId, cancellationToken);
        
        var totalCount = allContracts.Count;
        var pagedItems = allContracts
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<ContractDetailDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    // TASK-031: Tạo hợp đồng thuê
    public async Task<ContractDetailDto> CreateContractAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrganizationId == Guid.Empty) throw new ArgumentException("OrganizationId is required.");
        if (request.RoomId == Guid.Empty) throw new ArgumentException("RoomId is required.");
        if (request.ResidentId == Guid.Empty) throw new ArgumentException("ResidentId is required.");
        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException("EndDate must be after StartDate.");

        // Kiểm tra phòng có đang Available không
        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {request.RoomId} not found.");
        if (room.Status.Value == "Occupied")
            throw new InvalidOperationException("Room is already occupied.");

        var now = DateTime.UtcNow;
        var contract = new ContractEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            PropertyId = request.PropertyId,
            RoomId = request.RoomId,
            ResidentId = request.ResidentId,
            BrokerId = request.BrokerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DepositAmount = new Money(request.DepositAmount),
            BrokerCommission = request.BrokerCommission,
            Status = new Status("Active"),
            CreatedAt = now
        };

        await _unitOfWork.Contracts.AddAsync(contract, cancellationToken);

        // Cập nhật trạng thái phòng thành Occupied
        room.Status = new Status("Occupied");
        _unitOfWork.Rooms.Update(room);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = await _unitOfWork.Contracts.GetContractDetailByIdAsync(contract.Id, cancellationToken);
        return detail ?? _mapper.Map<ContractDetailDto>(contract);
    }

    // TASK-032: Cập nhật hợp đồng
    public async Task<ContractDetailDto> UpdateContractAsync(
        Guid contractId,
        UpdateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

        if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
        if (request.DepositAmount.HasValue) contract.DepositAmount = new Money(request.DepositAmount.Value);
        if (request.BrokerCommission.HasValue) contract.BrokerCommission = request.BrokerCommission.Value;
        if (request.CommissionStatus is not null) contract.CommissionStatus = request.CommissionStatus;

        _unitOfWork.Contracts.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Contracts.GetContractDetailByIdAsync(contractId, cancellationToken)
               ?? _mapper.Map<ContractDetailDto>(contract);
    }

    // TASK-033: Gia hạn hợp đồng
    public async Task<ContractDetailDto> RenewContractAsync(
        Guid contractId,
        RenewContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

        if (request.NewEndDate <= contract.EndDate)
            throw new InvalidOperationException("New end date must be after current end date.");

        contract.EndDate = request.NewEndDate;
        if (request.NewDepositAmount.HasValue)
            contract.DepositAmount = new Money(request.NewDepositAmount.Value);
        contract.Status = new Status("Active");

        _unitOfWork.Contracts.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Contracts.GetContractDetailByIdAsync(contractId, cancellationToken)
               ?? _mapper.Map<ContractDetailDto>(contract);
    }

    // TASK-034: Thanh lý hợp đồng
    public async Task<ContractDetailDto> TerminateContractAsync(
        Guid contractId,
        TerminateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

        contract.Status = new Status("Terminated");
        _unitOfWork.Contracts.Update(contract);

        // Cập nhật trạng thái phòng
        var newRoomStatus = request.NewRoomStatus ?? "Available";
        var room = await _unitOfWork.Rooms.GetByIdAsync(contract.RoomId, cancellationToken);
        if (room is not null)
        {
            room.Status = new Status(newRoomStatus);
            _unitOfWork.Rooms.Update(room);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Contracts.GetContractDetailByIdAsync(contractId, cancellationToken)
               ?? _mapper.Map<ContractDetailDto>(contract);
    }

    // TASK-035: Upload file hợp đồng
    public async Task<ContractDetailDto> UploadContractPdfAsync(
        Guid contractId,
        string pdfUrl,
        CancellationToken cancellationToken = default)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

        contract.ContractPdfUrl = pdfUrl;
        _unitOfWork.Contracts.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _unitOfWork.Contracts.GetContractDetailByIdAsync(contractId, cancellationToken)
               ?? _mapper.Map<ContractDetailDto>(contract);
    }

    // TASK-038: Theo dõi hợp đồng sắp hết hạn
    public Task<IReadOnlyList<ContractDetailDto>> GetExpiringSoonAsync(
        Guid organizationId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("organizationId is required.");
        return _unitOfWork.Contracts.GetExpiringSoonAsync(organizationId, daysThreshold, cancellationToken);
    }

    // TASK-NEW: Gửi thông báo nhắc nhở gia hạn
    public async Task SendRenewalNotificationAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

        if (contract.Status.Value != "Active")
        {
            throw new InvalidOperationException("Chỉ có thể gửi thông báo cho hợp đồng đang có hiệu lực.");
        }

        var room = await _unitOfWork.Rooms.GetByIdAsync(contract.RoomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {contract.RoomId} not found.");

        var property = await _unitOfWork.Properties.GetByIdAsync(contract.PropertyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Property {contract.PropertyId} not found.");

        var resident = await _unitOfWork.Residents.GetByIdAsync(contract.ResidentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Resident {contract.ResidentId} not found.");

        if (string.IsNullOrEmpty(resident.Email))
        {
            throw new InvalidOperationException("Khách thuê này chưa cập nhật địa chỉ Email trong hệ thống.");
        }

        await _publishEndpoint.Publish(new NotificationContractRenewEvent
        {
            ContractId = contract.Id,
            ResidentEmail = resident.Email,
            ResidentName = resident.FullName,
            RoomNumber = room.RoomNumber,
            PropertyName = property.PropertyName,
            EndDate = contract.EndDate.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
    }
}
