using AutoMapper;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Domain.ValueObject;

namespace NovaStay.Infrastructure.ServicesImple;

public sealed class AssetService : IAssetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // TASK-055: Danh sách tài sản + lọc
    public async Task<IReadOnlyList<AssetDto>> GetAssetsAsync(
        Guid organizationId,
        string? search = null,
        string? category = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("organizationId là bắt buộc.");

        return await _unitOfWork.Assets.GetAssetsWithCurrentStatusAsync(
            organizationId, search, category, status, cancellationToken);
    }

    // TASK-055 (derived): Tài sản trong phòng
    public async Task<IReadOnlyList<AssetDto>> GetAssetsByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        if (roomId == Guid.Empty)
            throw new ArgumentException("roomId là bắt buộc.");

        return await _unitOfWork.Assets.GetAssetsByRoomAsync(roomId, cancellationToken);
    }

    public async Task<PagedResult<AssetDto>> GetAssetsPagedAsync(
        Guid organizationId,
        string? search = null,
        string? category = null,
        string? status = null,
        string? location = null,
        int pageIndex = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("organizationId là bắt buộc.");

        var (items, total) = await _unitOfWork.Assets.GetAssetsPagedAsync(
            organizationId, search, category, status, location, pageIndex, pageSize, cancellationToken);

        return new PagedResult<AssetDto>
        {
            Items = items,
            TotalCount = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    // TASK-056: Thêm tài sản mới
    public async Task<AssetDto> CreateAssetAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrganizationId == Guid.Empty)
            throw new ArgumentException("organizationId là bắt buộc.");

        ValidateAssetFields(request.AssetName, request.Category, request.Brand, request.Model, request.AssetCode, request.BaseValue);
        ValidateWarranty(request.PurchaseDate, request.WarrantyExpiryDate);
        await EnsureAssetCodeAvailableAsync(request.OrganizationId, request.AssetCode, null, cancellationToken);

        var asset = new AssetEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            AssetName = new EntityName(request.AssetName.Trim()),
            Category = CleanOptional(request.Category),
            Brand = CleanOptional(request.Brand),
            Model = CleanOptional(request.Model),
            AssetCode = CleanOptional(request.AssetCode),
            PurchaseDate = request.PurchaseDate,
            WarrantyExpiryDate = request.WarrantyExpiryDate,
            BaseValue = request.BaseValue,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Assets.AddAsync(asset, cancellationToken);

        // Tạo bản ghi assignment ban đầu: tài sản đang trong kho
        var initialAssignment = new AssetAssignmentEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            AssetId = asset.Id,
            RoomId = null,       // Chưa gán phòng
            Status = "Good",
            Note = request.InitialNote ?? "Nhập kho",
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.AssetAssignments.AddAsync(initialAssignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Trả về DTO với current status
        var results = await _unitOfWork.Assets.GetAssetsWithCurrentStatusAsync(
            request.OrganizationId,
            cancellationToken: cancellationToken);

        return results.FirstOrDefault(a => a.Id == asset.Id)
               ?? _mapper.Map<AssetDto>(asset);
    }

    // TASK-056: Xóa mềm tài sản
    public async Task DeleteAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        if (asset.IsDeleted)
            throw new InvalidOperationException("Tài sản đã bị xóa.");

        // Kiểm tra nếu tài sản đang gán phòng thì từ chối xóa
        var latestAssignment = await _unitOfWork.AssetAssignments
            .GetLatestByAssetAsync(assetId, cancellationToken);

        if (latestAssignment?.RoomId != null)
            throw new InvalidOperationException(
                "Không thể xóa tài sản đang được gán vào phòng. Hãy thu hồi tài sản trước.");

        asset.IsDeleted = true;
        _unitOfWork.Assets.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // TASK-057: Cập nhật thông tin tài sản
    public async Task<AssetDto> UpdateAssetAsync(
        Guid assetId,
        UpdateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        if (asset.IsDeleted)
            throw new InvalidOperationException("Không thể cập nhật tài sản đã bị xóa.");

        ValidateAssetFields(request.AssetName, request.Category, request.Brand, request.Model, request.AssetCode, request.BaseValue);
        ValidateWarranty(request.PurchaseDate, request.WarrantyExpiryDate);
        await EnsureAssetCodeAvailableAsync(asset.OrganizationId, request.AssetCode, assetId, cancellationToken);

        asset.AssetName = new EntityName(request.AssetName.Trim());
        asset.Category = CleanOptional(request.Category);
        asset.Brand = CleanOptional(request.Brand);
        asset.Model = CleanOptional(request.Model);
        asset.AssetCode = CleanOptional(request.AssetCode);
        asset.PurchaseDate = request.PurchaseDate;
        asset.WarrantyExpiryDate = request.WarrantyExpiryDate;
        asset.BaseValue = request.BaseValue;

        _unitOfWork.Assets.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var results = await _unitOfWork.Assets.GetAssetsWithCurrentStatusAsync(
            asset.OrganizationId,
            cancellationToken: cancellationToken);

        return results.FirstOrDefault(a => a.Id == assetId)
               ?? _mapper.Map<AssetDto>(asset);
    }

    // TASK-058: Gán tài sản vào phòng
    public async Task<AssetHistoryDto> AssignAssetToRoomAsync(
        Guid assetId,
        AssignAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        if (asset.IsDeleted)
            throw new InvalidOperationException("Không thể gán tài sản đã bị xóa.");

        // Phòng phải tồn tại và thuộc cùng tổ chức với tài sản
        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng Id={request.RoomId}.");
        var property = await _unitOfWork.Properties.GetByIdAsync(room.PropertyId, cancellationToken);
        if (property is null || property.OrganizationId != asset.OrganizationId)
            throw new KeyNotFoundException("Phòng không thuộc tổ chức của tài sản.");

        // Kiểm tra tài sản đã gán phòng khác chưa
        var latestAssignment = await _unitOfWork.AssetAssignments
            .GetLatestByAssetAsync(assetId, cancellationToken);

        if (latestAssignment?.RoomId == request.RoomId)
            throw new InvalidOperationException("Tài sản đã được gán vào phòng này rồi.");

        if (latestAssignment?.RoomId != null && latestAssignment.RoomId != request.RoomId)
            throw new InvalidOperationException(
                "Tài sản đang được gán vào phòng khác. Hãy thu hồi trước khi gán vào phòng mới.");

        var assignment = new AssetAssignmentEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = asset.OrganizationId,
            AssetId = assetId,
            RoomId = request.RoomId,
            Status = "Good",
            Note = request.Note,
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.AssetAssignments.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var history = await _unitOfWork.AssetAssignments
            .GetHistoryByAssetAsync(assetId, cancellationToken);

        return history.FirstOrDefault(h => h.Id == assignment.Id)
               ?? _mapper.Map<AssetHistoryDto>(assignment);
    }

    // TASK-059: Thu hồi tài sản khỏi phòng
    public async Task<AssetHistoryDto> RevokeAssetFromRoomAsync(
        Guid assetId,
        RevokeAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        var latestAssignment = await _unitOfWork.AssetAssignments
            .GetLatestByAssetAsync(assetId, cancellationToken);

        if (latestAssignment?.RoomId == null)
            throw new InvalidOperationException("Tài sản đang không được gán vào phòng nào.");

        var assignment = new AssetAssignmentEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = asset.OrganizationId,
            AssetId = assetId,
            RoomId = null,       // Thu hồi về kho
            Status = "Good",
            Note = request.Note ?? "Thu hồi về kho",
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.AssetAssignments.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var history = await _unitOfWork.AssetAssignments
            .GetHistoryByAssetAsync(assetId, cancellationToken);

        return history.FirstOrDefault(h => h.Id == assignment.Id)
               ?? _mapper.Map<AssetHistoryDto>(assignment);
    }

    // TASK-060 & TASK-061: Cập nhật tình trạng / Ghi nhận hư hỏng
    public async Task<AssetHistoryDto> UpdateAssetStatusAsync(
        Guid assetId,
        UpdateAssetStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var validStatuses = new[] { "Good", "Damaged", "Maintenance", "Broken", "Working" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException(
                $"Trạng thái không hợp lệ. Các trạng thái được phép: {string.Join(", ", validStatuses)}");

        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        if (asset.IsDeleted)
            throw new InvalidOperationException("Không thể cập nhật tài sản đã bị xóa.");

        // Lấy phòng hiện tại (nếu có)
        var latestAssignment = await _unitOfWork.AssetAssignments
            .GetLatestByAssetAsync(assetId, cancellationToken);

        var assignment = new AssetAssignmentEntity
        {
            Id = Guid.NewGuid(),
            OrganizationId = asset.OrganizationId,
            AssetId = assetId,
            RoomId = latestAssignment?.RoomId, // Giữ nguyên phòng hiện tại
            Status = request.Status,
            Note = request.Note,
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.AssetAssignments.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var history = await _unitOfWork.AssetAssignments
            .GetHistoryByAssetAsync(assetId, cancellationToken);

        return history.FirstOrDefault(h => h.Id == assignment.Id)
               ?? _mapper.Map<AssetHistoryDto>(assignment);
    }

    // TASK-062: Lịch sử luân chuyển tài sản
    public async Task<IReadOnlyList<AssetHistoryDto>> GetAssetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _unitOfWork.Assets.GetByIdAsync(assetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tài sản {assetId} không tìm thấy.");

        return await _unitOfWork.AssetAssignments.GetHistoryByAssetAsync(assetId, cancellationToken);
    }

    // Task phát sinh: Thống kê
    public async Task<AssetStatisticsDto> GetStatisticsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("organizationId là bắt buộc.");

        return await _unitOfWork.Assets.GetStatisticsAsync(organizationId, cancellationToken);
    }

    // Hạn bảo hành không được trước ngày mua/nhập kho.
    private static void ValidateWarranty(DateOnly? purchaseDate, DateOnly? warrantyExpiryDate)
    {
        if (purchaseDate.HasValue && warrantyExpiryDate.HasValue && warrantyExpiryDate.Value < purchaseDate.Value)
            throw new ArgumentException("Hạn bảo hành không được trước ngày mua nhập kho.");
    }

    // Mã tài sản là duy nhất trong mỗi cơ sở (bỏ qua mã trống)
    private async Task EnsureAssetCodeAvailableAsync(Guid organizationId, string? assetCode, Guid? excludeAssetId, CancellationToken cancellationToken)
    {
        var code = CleanOptional(assetCode);
        if (code is null) return;

        if (await _unitOfWork.Assets.AssetCodeExistsAsync(organizationId, code, excludeAssetId, cancellationToken))
            throw new InvalidOperationException($"Mã tài sản \"{code}\" đã tồn tại trong cơ sở này.");
    }

    // Giới hạn theo cột database: AssetName 100, Category/Brand/Model/AssetCode 50, BaseValue decimal(18,2)
    private const decimal MaxAssetBaseValue = 9_999_999_999_999_999.99m;

    private static string? CleanOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateAssetFields(string? assetName, string? category, string? brand, string? model, string? assetCode, decimal? baseValue)
    {
        var name = assetName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên tài sản là bắt buộc.");
        if (name.Length > 100)
            throw new ArgumentException("Tên tài sản không được vượt quá 100 ký tự.");
        if (CleanOptional(category)?.Length > 50)
            throw new ArgumentException("Danh mục không được vượt quá 50 ký tự.");
        if (CleanOptional(brand)?.Length > 50)
            throw new ArgumentException("Thương hiệu không được vượt quá 50 ký tự.");
        if (CleanOptional(model)?.Length > 50)
            throw new ArgumentException("Model không được vượt quá 50 ký tự.");
        if (CleanOptional(assetCode)?.Length > 50)
            throw new ArgumentException("Mã tài sản không được vượt quá 50 ký tự.");
        if (baseValue is < 0)
            throw new ArgumentException("Giá trị tài sản không được âm.");
        if (baseValue > MaxAssetBaseValue)
            throw new ArgumentException("Giá trị tài sản vượt quá giới hạn cho phép.");
    }
}
