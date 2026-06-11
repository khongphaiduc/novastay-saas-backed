# Host App Backend

Backend cho hệ thống quản lý bất động sản/cho thuê, xây bằng ASP.NET Core 8, Entity Framework Core SQL Server và AutoMapper.

Mục tiêu của tài liệu này là giúp thành viên mới hiểu nhanh codebase: project gồm những layer nào, class chính làm gì, luồng request chạy ra sao và cách cấu hình database local.

## Công Nghệ Chính

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8 với SQL Server
- AutoMapper
- xUnit cho unit test
- DotNetEnv để load biến môi trường từ file `.env`

## Cấu Trúc Solution

```text
host-app.sln
├── Host.API              # Entry point Web API: Program.cs, Controllers, appsettings
├── Host.Application      # Use case/service, DTO, interface repository, mapping DTO
├── Host.Domain           # Entity nghiệp vụ và Value Object
├── Host.Infrastructure   # EF Core DbContext, model database, repository, UnitOfWork, migrations
└── Host.UnitTests        # Unit test cho Domain/Application/Infrastructure mapping
```

Quan hệ dependency hiện tại:

```text
Host.API
  ├── Host.Application
  └── Host.Infrastructure

Host.Infrastructure
  ├── Host.Application
  └── Host.Domain

Host.Application
  └── Host.Domain

Host.Domain
  └── Không phụ thuộc project khác
```

## Vai Trò Từng Layer

### 1. Host.API

Đây là layer ngoài cùng, nhận HTTP request và trả HTTP response.

Các file/class quan trọng:

- `Program.cs`
  - Tạo `WebApplication`.
  - Load file `.env` từ `Host.Infrastructure/Config/.env` nếu file tồn tại.
  - Đăng ký dependency injection qua `AddInfrastructure(...)`.
  - Đăng ký AutoMapper profile:
    - `ApplicationMappingProfile`
    - `DatabaseModelMappingProfile`
  - Đăng ký application service mẫu: `ISampleDataService -> SampleDataService`.
  - Bật controller routing.
- `Controllers/SampleDataController.cs`
  - Controller mẫu.
  - Endpoint hiện có: `GET /api/SampleData/tenants?take=20`.
  - Controller chỉ nên điều phối request/response, không viết trực tiếp logic database.
- `appsettings.json`, `appsettings.Development.json`
  - Cấu hình logging, host và connection string mẫu.
  - Lưu ý: DI hiện tại đang đọc `SQLString`, không đọc `ConnectionStrings:DefaultConnection`.

Khi thêm API mới, nên tạo controller ở `Host.API/Controllers`, inject service từ `Host.Application`, rồi trả DTO.

### 2. Host.Application

Layer này chứa logic ứng dụng, contract giữa API và Infrastructure, DTO và mapping giữa Domain Entity với DTO.

Các thư mục/class quan trọng:

- `DTOs/*Dto.cs`
  - Dữ liệu trả ra/nhận vào ở boundary của application.
  - Ví dụ: `TenantDto`, `RoomDto`, `BookingDto`, `InvoiceDto`.
- `Services/ISampleDataService.cs`
  - Interface cho service xử lý use case.
- `Services/SampleDataService.cs`
  - Service mẫu.
  - Inject `ITenantRepository` và `IMapper`.
  - Lấy tenant từ repository, sort theo `CreatedAt`, giới hạn số lượng record, map sang `TenantDto`.
- `Common/Interfaces/IRepository.cs`
  - Generic repository contract cho domain entity.
  - Có các operation: `Query`, `GetByIdAsync`, `ListAsync`, `FindAsync`, `AddAsync`, `Update`, `Remove`.
- `Common/Interfaces/IUnitOfWork.cs`
  - Gom toàn bộ repository theo nghiệp vụ: `Tenants`, `Rooms`, `Bookings`, `Invoices`, ...
  - Cung cấp `SaveChangesAsync()` để commit thay đổi xuống database.
- `Common/Interfaces/I*Repository.cs`
  - Repository interface riêng cho từng aggregate/entity.
  - Nếu entity cần query đặc thù, thêm method vào interface riêng. Ví dụ `ITenantRepository.GetByOwnerEmailAsync(...)`.
- `Common/Mappings/ApplicationMappingProfile.cs`
  - AutoMapper profile map giữa `Domain Entity <-> DTO`.
  - Có converter cho Value Object như `EmailAddress`, `PhoneNumber`, `Money`, `Status`, `Code`.

Nguyên tắc: Application không biết EF Core hoạt động thế nào. Application chỉ gọi interface repository.

### 3. Host.Domain

Layer lõi nghiệp vụ. Không phụ thuộc API, Infrastructure hay framework web.

Các thư mục/class quan trọng:

- `Entities/Entity.cs`
  - Base class có `Guid Id`.
- `Entities/*Entity.cs`
  - Domain entity đại diện nghiệp vụ: `TenantEntity`, `PropertyEntity`, `RoomEntity`, `BookingEntity`, `ContractEntity`, `InvoiceEntity`, ...
  - Các entity dùng Value Object cho một số field để code rõ nghĩa hơn.
- `ValueObject/*.cs`
  - Kiểu dữ liệu nhỏ, có ý nghĩa nghiệp vụ:
    - `EmailAddress`
    - `PhoneNumber`
    - `EntityName`
    - `Money`
    - `Status`
    - `Code`
    - `DateRange`
    - `UrlValue`

Nguyên tắc: Domain nên chứa rule nghiệp vụ quan trọng và không nên gọi database, HTTP, appsettings hoặc service bên ngoài.

### 4. Host.Infrastructure

Layer này chứa chi tiết kỹ thuật: EF Core, SQL Server, model database, repository implementation, UnitOfWork, migrations và DI.

Các thư mục/class quan trọng:

- `ContextDB/HostContext.cs`
  - EF Core `DbContext`.
  - Khai báo `DbSet` cho các bảng: `Tenants`, `Users`, `Properties`, `Rooms`, `Bookings`, `Invoices`, ...
  - Cấu hình key, index, relationship, column type, default value trong `OnModelCreating`.
  - Có logic set default SQL `newsequentialid()` cho các property `Id` kiểu `Guid`.
- `Models/*.cs`
  - Database model được EF Core map trực tiếp với bảng.
  - Đây là model persistence, khác với `Host.Domain.Entities`.
- `Persistence/DI/DependencyInjection.cs`
  - Extension method `AddInfrastructure(...)`.
  - Đăng ký `HostContext` với SQL Server.
  - Đăng ký `IDatabaseModelMapper<,>`, toàn bộ repository và `IUnitOfWork`.
  - Hiện tại lấy connection string từ key cấu hình `SQLString`.
- `Persistence/Repositories/Repository.cs`
  - Generic repository implementation.
  - Map `Domain Entity <-> Database Model` bằng `IDatabaseModelMapper`.
  - Query đang dùng `AsNoTracking()`.
- `Persistence/Repositories/EntityRepositories.cs`
  - Repository cụ thể cho từng entity.
  - Phần lớn kế thừa generic `Repository<TDomain, TDatabase>`.
  - `TenantRepository` có thêm method riêng `GetByOwnerEmailAsync`.
- `Persistence/Repositories/UnitOfWork.cs`
  - Implement `IUnitOfWork`.
  - Gom các repository và gọi `_context.SaveChangesAsync(...)`.
- `Persistence/Mapping/DatabaseModelMapper.cs`
  - Wrapper quanh AutoMapper để chuyển đổi giữa Domain Entity và Database Model.
- `Persistence/Mapping/DatabaseModelMappingProfile.cs`
  - AutoMapper profile map `Host.Infrastructure.Models.* <-> Host.Domain.Entities.*`.
- `Migrations/*`
  - Migration EF Core hiện có cho schema SQL Server.

Nguyên tắc: Infrastructure được phép biết EF Core và database, nhưng Application/API chỉ nên làm việc qua interface.

### 5. Host.UnitTests

Project test dùng xUnit.

Các test hiện có:

- `Domain/ValueObjectTests.cs`
  - Test behavior của Value Object.
- `Application/ApplicationMappingProfileTests.cs`
  - Test mapping giữa Domain và DTO.
- `Infrastructure/DatabaseModelMappingProfileTests.cs`
  - Test mapping giữa Database Model và Domain Entity.

## Luồng Request Mẫu

Ví dụ endpoint `GET /api/SampleData/tenants?take=20`:

```text
HTTP Request
  -> Host.API/Controllers/SampleDataController
  -> Host.Application/Services/SampleDataService
  -> Host.Application/Common/Interfaces/ITenantRepository
  -> Host.Infrastructure/Persistence/Repositories/TenantRepository
  -> Host.Infrastructure/ContextDB/HostContext
  -> SQL Server
```

Khi dữ liệu đi ra:

```text
SQL row
  -> Infrastructure Model
  -> Domain Entity
  -> DTO
  -> HTTP Response
```

## Setup Local

### 1. Yêu Cầu Môi Trường

- .NET SDK 8.x
- SQL Server local hoặc remote
- Visual Studio 2022 / Rider / VS Code
- EF Core CLI nếu cần chạy migration:

```powershell
dotnet tool install --global dotnet-ef
```

Nếu đã cài rồi:

```powershell
dotnet tool update --global dotnet-ef
```

### 2. Restore Và Build

Tại thư mục root `host-app`:

```powershell
dotnet restore
dotnet build
```

### 3. Cấu Hình Database Connection

Code hiện tại trong `Host.Infrastructure/Persistence/DI/DependencyInjection.cs` đọc connection string bằng key:

```csharp
configuration["SQLString"]
```

Vì vậy cách setup được khuyến nghị là tạo file:

```text
Host.Infrastructure/Config/.env
```

Nội dung ví dụ cho SQL Server local dùng Windows Authentication:

```env
SQLString=Server=localhost;Database=host;Trusted_Connection=True;TrustServerCertificate=True
```

Nội dung ví dụ cho SQL Server dùng username/password:

```env
SQLString=Server=localhost;Database=host;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True
```

`Program.cs` của `Host.API` sẽ tự tìm và load file `.env` ở đường dẫn trên khi API khởi động.

Lưu ý: `Host.API/appsettings.Development.json` đang có `ConnectionStrings:DefaultConnection`, nhưng key này chưa được `AddInfrastructure(...)` sử dụng. Nếu muốn dùng `DefaultConnection`, cần sửa DI sang:

```csharp
var connectionString = configuration.GetConnectionString("DefaultConnection");
```

### 4. Tạo Database Bằng Migration

Project đã có migration trong `Host.Infrastructure/Migrations`.

Chạy lệnh sau từ root:

```powershell
dotnet ef database update --project Host.Infrastructure --startup-project Host.API
```

Nếu cần tạo migration mới sau khi đổi model:

```powershell
dotnet ef migrations add TenMigration --project Host.Infrastructure --startup-project Host.API
dotnet ef database update --project Host.Infrastructure --startup-project Host.API
```

### 5. Chạy API

```powershell
dotnet run --project Host.API
```

Theo `launchSettings.json`, API có thể chạy tại:

- `http://localhost:5050`
- `https://localhost:7286`

Endpoint kiểm tra nhanh:

```text
GET http://localhost:5050/api/SampleData/tenants?take=20
```

### 6. Chạy Test

```powershell
dotnet test
```

## Quy Ước Khi Thêm Tính Năng Mới

Ví dụ thêm tính năng quản lý `Room`:

1. Kiểm tra domain entity trong `Host.Domain/Entities/RoomEntity.cs`.
2. Nếu cần request/response mới, thêm DTO trong `Host.Application/DTOs`.
3. Thêm hoặc cập nhật mapping trong `ApplicationMappingProfile`.
4. Thêm method cần thiết vào `IRoomRepository` nếu query chung trong `IRepository` chưa đủ.
5. Implement method đó trong repository tương ứng ở `Host.Infrastructure/Persistence/Repositories/EntityRepositories.cs`.
6. Viết service/use case trong `Host.Application/Services`.
7. Đăng ký service trong `Host.API/Program.cs` hoặc tạo extension DI riêng khi số lượng service tăng.
8. Tạo controller trong `Host.API/Controllers`.
9. Viết test cho mapping, domain rule hoặc service logic.

## Lưu Ý Kỹ Thuật

- Không đưa logic nghiệp vụ trực tiếp vào Controller.
- Không để Application phụ thuộc EF Core hoặc `Host.Infrastructure.Models`.
- Domain entity và Infrastructure model là hai loại khác nhau:
  - Domain entity nằm trong `Host.Domain/Entities`.
  - Database model nằm trong `Host.Infrastructure/Models`.
- Sau khi thêm entity/model mới, cần cập nhật:
  - `HostContext`
  - repository interface
  - repository implementation
  - `IUnitOfWork` và `UnitOfWork`
  - AutoMapper profile tương ứng
  - migration nếu schema database thay đổi
- `Repository.Query()` hiện map dữ liệu sang memory bằng `AsEnumerable()` rồi mới trả `IQueryable<TDomain>`. Với bảng lớn, nên cẩn thận vì filter có thể chạy ở memory thay vì SQL.

