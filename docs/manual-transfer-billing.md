# Thu tiền phòng và dịch vụ bằng chuyển khoản

Tài liệu này mô tả phương thức Manual: chuyển khoản được chủ trọ đối chiếu và xác nhận thủ công. Luồng PayOS demo và thông báo SignalR bổ sung xem `antigravity-payos-signalr-frontend.md` và `payos-demo-operations.md`; Manual vẫn giữ nguyên cách sử dụng.

## Sử dụng

1. Khởi động lại backend để áp dụng migration `AddManualTransferBilling`, sau đó chạy frontend phiên bản mới. Backend hiện tự chạy migration khi khởi động; kiểm tra database đích trước khi chạy.
2. Chủ trọ vào **Thu chi và công nợ → Cấu hình tài khoản nhận tiền**. Nhập ngân hàng, số tài khoản, tên chủ tài khoản; có thể tải ảnh QR PNG/JPG tối đa 2 MB. Kiểm tra QR khớp với tài khoản trước khi lưu. QR tải lên là ảnh tĩnh, không tự thêm số tiền hoặc nội dung hóa đơn.
3. Bấm **Lập phiếu thu**, chọn nhà trọ, kỳ, hạn thanh toán; tick khoản thu và chọn một/nhiều/tất cả phòng. Xem số tiền từng phòng trước khi phát hành.
4. Người đại diện trong hợp đồng của phòng mở tab **Hóa đơn**, xem tài khoản, sao chép nội dung chuyển khoản, tải ảnh QR. Sau khi chuyển khoản, bấm **Tôi đã chuyển khoản**.
5. Chủ trọ kiểm tra giao dịch ngân hàng rồi bấm **Xác nhận đã nhận tiền → Xác nhận đã nhận đủ tiền**. Có thể xác nhận trực tiếp nếu người thuê chưa báo chuyển khoản. Ghi chú tối đa 500 ký tự.
6. Trạng thái chuyển từ `Unpaid` → `AwaitingConfirmation` → `Paid`. Nút báo chuyển khoản không tăng doanh thu. Chỉ xác nhận của chủ trọ mới tạo khoản thực thu và giảm công nợ. Hai phía có nút tải lại; dữ liệu cũng tải lại khi quay về tab trình duyệt, không có kết nối thời gian thực.

## Quy tắc tính tiền và lưu trữ

- Mỗi phòng có một người đại diện trả toàn bộ. Cần đúng một hợp đồng `Active` thuộc tổ chức, giao với kỳ thu. Nếu có nhiều hợp đồng cùng kỳ, cần xử lý dữ liệu hợp đồng trước; hệ thống không tự chọn người nộp.
- Tiền phòng lấy `Room.BasePrice` theo tháng, không tự chia theo ngày. Model hợp đồng hiện chưa có giá thuê riêng.
- Dịch vụ cố định lấy `RoomService.PriceOverride` hoặc `PropertyService.DefaultPrice`, chỉ với dịch vụ được gán và còn hoạt động.
- Dịch vụ có đồng hồ lấy lượng tiêu thụ trong `UtilityReading` đúng kỳ, nhân đơn giá đang cấu hình (`RoomService.PriceOverride` nếu có, nếu không lấy `PropertyService.DefaultPrice`). Đơn giá chỉ chốt khi phát hành hóa đơn; số tiền tạm tính trong lần ghi chỉ số cũ không thay thế giá đang cấu hình. `UtilityTariff` cũ chưa có màn cấu hình không được dùng để ghi đè giá trên màn dịch vụ. Thiếu chỉ số thì chặn khoản đó; vẫn có thể chỉ thu tiền phòng/dịch vụ khác. Dịch vụ theo lần/người hoặc đơn vị đo nhưng chưa có dữ liệu định lượng không được suy đoán thành phí cố định.
- Số tiền thanh toán làm tròn đến đồng. Lưu đơn giá/số lượng trong `InvoiceLine`; thay đổi giá sau phát hành không đổi tổng tiền hóa đơn đã lập.
- Chỉ cho thu các khoản chưa phát hành trong cùng phòng/kỳ. Có unique index tại database chống yêu cầu tạo trùng đồng thời. Có thể phát hành phần dịch vụ còn lại trong đợt khác.
- Mỗi đợt lưu hóa đơn là một transaction: lỗi ở bất kỳ phòng nào thì không lưu một phần. Bảng xem trước hiển thị lỗi để bỏ chọn hoặc bổ sung dữ liệu rồi thử lại.
- Tài khoản ngân hàng được lưu thành các phiên bản bất biến; hóa đơn trỏ đến phiên bản lúc phát hành. Thay đổi số tài khoản không tự dùng lại QR cũ. Ảnh chỉ được tải qua endpoint có xác thực và kiểm tra quyền chủ trọ/người nộp.
- Khi xác nhận, cập nhật hóa đơn, `PaymentReceipt` và phiếu thu `IncomeReceipt` dùng cho sổ thu chi hiện tại trong cùng lần lưu. Rowversion và unique index thanh toán bảo vệ việc xác nhận đồng thời. Lưu tài khoản chủ trọ xác nhận, thời gian, ghi chú. Thử lại hóa đơn đã trả không tạo thêm phiếu thu.
- Không chuyển đổi phiếu thu lịch sử thành hóa đơn mới. Phiếu thu cũ vẫn hiển thị trong phần lịch sử riêng; cần tránh tự lập lại những khoản đã thu thủ công trước khi dùng luồng này.
- Migration tạo schema mới và đồng bộ bộ lọc index số điện thoại đã có trong model (`Phone IS NOT NULL AND Phone <> ''`). Không xóa dữ liệu hoặc secret.

## API

Tất cả endpoint bên dưới bắt buộc JWT. Tổ chức được xác định từ chủ tài khoản trong database, không tin organization ID do trình duyệt gửi.

| Endpoint | Quyền |
| --- | --- |
| `GET/PUT /api/billing/bank-account` | BusinessOwner; PUT multipart |
| `GET /api/billing/properties` | BusinessOwner |
| `GET /api/billing/preview?propertyId=...&period=yyyy-MM` | BusinessOwner |
| `GET/POST /api/billing/invoices` | BusinessOwner |
| `POST /api/billing/invoices/{id}/confirm` | BusinessOwner sở hữu hóa đơn |
| `GET /api/billing/me/invoices` | Resident người đại diện |
| `POST /api/billing/me/invoices/{id}/report-transfer` | Resident người đại diện |
| `GET /api/billing/bank-accounts/{id}/qr` | Chủ tài khoản hoặc người đại diện có hóa đơn dùng phiên bản này |

Không cần thêm API key hay secret mới. Chưa triển khai lên VPS khi chỉ thay đổi mã nguồn local.
