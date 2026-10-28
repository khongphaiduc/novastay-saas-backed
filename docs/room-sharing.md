# Người ở ghép — triển khai và kiểm thử local

## Mô hình

- Mỗi phòng giữ một hợp đồng đang hiệu lực. `Contracts.ResidentId` tiếp tục là người đại diện ký kết/thanh toán.
- Bảng mới `ContractCoResidents` chỉ chứa người ở cùng: khóa `(ContractId, ResidentId)`, thời điểm vào và `LeftAt` khi rời phòng. Thêm lại sẽ cập nhật dòng cũ; đây không phải nhật ký đầy đủ mọi lần ra/vào.
- Không nhân đôi tiền phòng, không tạo hợp đồng/hóa đơn riêng cho người ở ghép. Luồng QR, xác nhận thanh toán và SignalR tiếp tục gửi cho người đại diện và chủ trọ.
- Người ở ghép xem được phòng qua API `residents/me/room`; không được thêm quyền xem hóa đơn, QR hoặc PDF hợp đồng riêng của người đại diện. Danh sách hợp đồng cá nhân vẫn chỉ chứa hợp đồng đứng tên họ.
- Thanh lý hợp đồng làm liên kết ở ghép hết hiệu lực theo trạng thái hợp đồng; dữ liệu cũ giữ lại. Khi gia hạn/kích hoạt lại, hệ thống kiểm tra lại xung đột nếu hợp đồng có người ở ghép.

## API dành cho chủ trọ

Tất cả yêu cầu JWT `BusinessOwner` và kiểm tra `OwnerAccountId` của hợp đồng/tổ chức từ database, không tin organizationId do frontend gửi để cấp quyền.

| Phương thức | Đường dẫn | Kết quả |
|---|---|---|
| GET | `/api/organizations/{organizationId}/room-occupancies` | Các phòng có hợp đồng Active và toàn bộ người trong phòng |
| GET | `/api/contracts/{id}/occupants` | Đại diện, người ở cùng và sức chứa |
| GET | `/api/contracts/{id}/co-resident-candidates?search=&page=1&pageSize=20` | Cư dân ACTIVE cùng trọ, tài khoản hoạt động, chưa có phòng; có phân trang |
| PUT | `/api/contracts/{id}/co-residents` | Thay thế danh sách người ở cùng, body `{"residentIds":["GUID"]}`; không gửi đại diện |

PUT dùng `residentIds: []` để cho toàn bộ người ở ghép rời phòng, giữ nguyên người đại diện. Sai chủ trọ trả 404, dữ liệu sai trả 400, xung đột/sức chứa trả 409. PUT giới hạn 20 lần/phút/tài khoản; có thể cấu hình `Security__RateLimits__RoomOccupancy__PermitLimit` và `WindowSeconds`.

## Kiểm tra dữ liệu

`HostContext.SaveChanges` kiểm tra các thay đổi liên quan đến ở ghép, gồm cả việc sửa sức chứa, đổi membership hoặc cho một người đang ở ghép đứng tên phòng khác. Mỗi cư dân phải thuộc đúng trọ, tài khoản hoạt động, không ở hai phòng và không xuất hiện hai lần trong một phòng. Phòng phải đúng property/organization, còn sử dụng và đủ sức chứa.

Trên SQL Server, kiểm tra và ghi chạy trong transaction Serializable khi caller chưa mở transaction. Caller đã mở transaction chịu trách nhiệm giữ mức cô lập phù hợp. Xếp đồng thời có thể bị SQL Server chọn làm deadlock victim; API trả thông báo tải lại và thử lại. Các thao tác SQL trực tiếp bỏ qua kiểm tra của ứng dụng, vì vậy script chuyển dữ liệu phải có kiểm tra tương đương và transaction riêng.

Không thay đổi hàng loạt quy tắc hợp đồng cũ chỉ có một người. Các bất thường cũ như nhiều hợp đồng đại diện/phòng vẫn bị phần lập hóa đơn chặn như trước.

## Giao diện

Chủ trọ vào **Hợp đồng → Chi tiết → Người trong phòng → Quản lý người ở ghép**. Có tìm kiếm/phân trang cư dân trống phòng, thêm/bỏ chọn và lưu. Đại diện không có nút xóa trong màn này. Muốn tăng số người phải tăng sức chứa phòng trước.

Quản lý cư dân dùng API room-occupancies để hiện đúng phòng cho cả đại diện và người ở cùng. Nếu backend cũ chưa có API này, frontend tạm dùng cách lấy phòng từ hợp đồng như trước; người ở ghép cần backend mới mới hiển thị đúng.

## Database local và migration

Migration mới: `20261028034605_AddContractCoResidents`, chỉ tạo bảng/index/FK, không seed hoặc di chuyển dữ liệu thật.

Đã tạo database kiểm thử độc lập trên Windows LocalDB:

```text
Server=(localdb)\MSSQLLocalDB
Database=NovaStayCoResidentsLocal20261028
Authentication=Windows Authentication
```

Database này chứa tài khoản giả lập với email `example.invalid`, không có mật khẩu đăng nhập thật và không sao chép hồ sơ cư dân production. Bộ mô phỏng chuyển 31/32 người, tạo 12 phòng hai người giá 3 triệu và 7 phòng một người giá 2 triệu, 19 hợp đồng, 12 liên kết ở ghép. Giá phòng tổng 50 triệu/tháng. Tiền cọc mẫu 0 và ngày hợp đồng chỉ là dữ liệu kiểm thử, không phải thiết lập cho cư dân thật.

Mở database bằng SSMS để xem bảng Rooms, Contracts, ContractCoResidents và ResidentMemberships. Harness một lần nằm ở `artifacts/co-residents-local` trong workspace cha, không thuộc mã deploy. Harness không đọc `.env`, chỉ nhận kết nối LocalDB cố định, không xóa dữ liệu và từ chối seed lại khi database đã có tổ chức.

Theo yêu cầu kiểm thử trực tiếp, `.env` local hiện đã trỏ lại database VPS `NovaStayDatabase` qua tunnel `127.0.0.1,14433`. Migration `20261028034605_AddContractCoResidents` đã được áp dụng riêng sau khi backup COPY_ONLY/CHECKSUM và RESTORE VERIFYONLY thành công. Bản backup nằm trong `/var/opt/mssql/`, đường dẫn cụ thể được lưu ở `.security-private/vps-co-residents-backup-path.txt`. Chưa chuyển 31 cư dân thật hoặc tạo hợp đồng/hóa đơn mới. File production và image API trên VPS giữ nguyên. Mọi thao tác ghi từ API local hiện sẽ tác động vào database thật. Giữ worker thanh toán local tắt để tránh lấy sự kiện của VPS. Database LocalDB giả lập vẫn được giữ độc lập để kiểm thử hồi quy.

Local `.env` đã đặt `Database__ApplyMigrationsOnStartup=false`. Development mặc định không tự migrate; nếu schema còn thiếu, startup dừng với thông báo yêu cầu migrate database local. Production vẫn mặc định tự migrate như trước khi deploy image mới, vì vậy phải review migration và sao lưu trước lần triển khai đó. Design-time factory chỉ dùng cấu hình metadata LocalDB, không khởi động API hoặc đọc secret production khi tạo migration.

## Đã kiểm chứng

- Kiểm thử InMemory: phân quyền, danh sách ứng viên, sức chứa, người khác trọ/bị khóa, xếp trùng, rời/vào lại, gia hạn/thanh lý, phòng cá nhân, quyền hóa đơn đại diện.
- SQL LocalDB thật: chạy toàn bộ migration, seed/move dữ liệu giả lập 31 người/19 phòng trong transaction, preview ra 19 tiền phòng tổng 50 triệu.
- Hai SQL context cùng xếp một người vào hai phòng: chỉ một giao dịch thành công; sau thử đã khôi phục phân bổ mẫu 31 người/19 phòng.
- Chưa triển khai image API/frontend mới lên VPS; mới thêm bảng/schema ở ghép theo yêu cầu kết nối thử trực tiếp. Chưa chuyển hoặc phát hành hóa đơn cho 31 tài khoản thật. Thu phí theo đầu người vẫn theo quy tắc định lượng cũ; không tự suy diễn số lượng từ sức chứa phòng.
