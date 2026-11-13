# Kết quả sửa lỗi theo handoff — 02/11/2026

## Phạm vi

Sửa trên source local của cả backend và frontend. Không cập nhật dữ liệu VPS, không đổi secret, cấu hình PayOS/SignalR, cách chia tiền theo ngày hoặc lịch sử hóa đơn. Không có migration/schema change mới.

## Các nhóm đã xử lý

| Mã handoff | Thay đổi |
|---|---|
| SEC-01 | OwnerData filter kiểm tra owner theo database trước action, gồm ID trong route/query và DTO; bảo vệ các API phòng, hợp đồng, cư dân, cơ sở, tổ chức, tài sản, đồng hồ, thu chi và bảo trì. Body không được ghi đè việc kiểm tra ID trong route. Endpoint financial-transactions cũ được bổ sung yêu cầu đăng nhập BusinessOwner. |
| FE-01 | API `/api/rooms/summary?propertyId=...` đếm toàn cơ sở; FE dùng summary cho KPI/tab counts, không đếm trang hiện tại. |
| FE-02 | BE hiểu các bộ lọc hợp đồng tiếng Việt mà FE đang dùng; áp dụng điều kiện trạng thái/ngày trước count và phân trang. Đồng bộ ngày tham chiếu UTC+7 cho daysUntilExpiry. |
| LOG-01 | Bất biến occupancy áp dụng cả hợp đồng một đại diện: đúng org/property, membership hoạt động, account hoạt động, không trùng người/phòng, không vượt capacity. Giữ transaction Serializable khi kiểm tra và ghi. |
| LOG-02 | Chỉ gia hạn hợp đồng Active; cập nhật phòng Occupied; thanh lý lặp không mở lại phòng đã có hợp đồng mới. Kiểm tra còn hợp đồng khác trước khi đặt Available. |
| LOG-03 | Sửa ngày kiểm tra khoảng ngày cuối cùng trước khi cập nhật; không ghi dữ liệu khi EndDate <= StartDate. |
| LOG-04 | Xóa mềm phòng còn hợp đồng Active bị validator chặn; trả lỗi 409 có hướng xử lý thay vì để phòng biến mất khỏi billing. |
| FE-03 | Tạo/sửa phòng tải lại trang 1 và thống kê; xóa phần tử cuối trang chuyển về trang trước hợp lệ và tải lại dữ liệu. |
| PERF-01 | Room/contract list thực hiện Count + OrderBy ổn định + Skip/Take tại SQL; giới hạn pageSize 1–200 và offset không tràn int. API `/api/contracts/summary?organizationId=...` thay vòng lặp tải hết hợp đồng để tính KPI. |
| FE-04 | Billing trả và FE hiển thị nguyên nhân thực: chưa có hợp đồng, chưa bắt đầu trong kỳ, ngoài kỳ hoặc có nhiều hợp đồng. Không tự gán đại diện để vượt điều kiện. |
| FE-05 | Bỏ nơi ở/lời mời giả và bỏ cache localStorage dùng chung; có loading/error/retry khi API lỗi. |
| UX-01 | Ẩn ô tìm phòng và chuông topbar chưa có chức năng, giữ chức năng tìm kiếm thật trong từng màn. |

### Bổ sung trong quá trình kiểm tra

- Chặn proxy ảnh nhận URL tùy ý: chỉ chấp nhận nguồn và prefix kho lưu trữ đã cấu hình, không theo redirect, giới hạn 10 MB và loại MIME ảnh; không trả exception nội bộ ra client.
- Bỏ phản hồi chứa inner exception SQL tại endpoint bảo trì.
- Sửa vòng lặp fetchProfile → setAccount → fetchProfile ở màn nơi ở; chỉ cập nhật tên khi thay đổi, kiểm tra access token trước khi ghi storage để tránh phản hồi phiên cũ khôi phục dữ liệu.
- Giữ luồng tìm/mời cư dân chưa có trọ: invitation request không yêu cầu membership có sẵn, nhưng tổ chức nhận lời mời vẫn phải thuộc chủ đang đăng nhập.
- Dọn import không dùng và lỗi lint trong các file frontend đã chỉnh; không auto-fix toàn repository.

## Kiểm chứng

- Backend: bộ test cũ và test bổ sung về owner, route/body, upload, proxy, ngày, phân trang, occupancy; **247/247 đạt**.
- HTTP/MVC thật trên localhost: không đăng nhập bị 401; chủ khác bị 404 trước service ở read/update/upload; owner đúng đọc được.
- SQL Server LocalDB cô lập `(localdb)\MSSQLLocalDB`, database `NovaStayCoResidentsLocal20261028`: dữ liệu tổng hợp, không đọc file .env; kiểm tra concurrency ở ghép, phân trang 19 hợp đồng/phòng và SQL translation của API thống kê.
- FE: **19/19 unit tests đạt**; lint 7 file đã sửa đạt; production build; Playwright/Chrome với API giả ở 360/390/768/1440px. Kiểm tra KPI 77 phòng/72 đang ở, bộ lọc hợp đồng, lỗi tải nơi ở, không hiện cache cũ và không lặp tải hồ sơ.
- Ảnh kiểm tra: `ProjectEXE/artifacts/audit-ui/`.
- Test script UI và SQL: `ProjectEXE/artifacts/audit-ui/check.cjs`, `ProjectEXE/artifacts/audit-sql/`.

## Triển khai

1. Review diff, test và rà soát secret trước commit.
2. Sao lưu quy trình production như hiện tại; triển khai **backend trước frontend** do FE cần hai API summary mới.
3. Không cần migration mới cho bản sửa này. Không bật background workers local khi dùng chung DB production.
4. Smoke test bằng tài khoản chủ trọ thật: phòng, KPI, tìm/lọc/trang, tạo/sửa/thanh lý/gia hạn hợp đồng, mời cư dân, ở ghép, lập phiếu thu và ghi chỉ số.
5. Kiểm tra lại luồng PayOS demo/SignalR sau deploy trong môi trường được cho phép. Đợt này không thực hiện chuyển khoản thật hoặc thử OAuth thật.
6. Không chạy script sửa dữ liệu lịch sử tự động. Nếu dữ liệu legacy vi phạm quy tắc mới (hợp đồng trùng, membership thiếu, ngày không hợp lệ), cần báo cụ thể và review riêng trước khi chỉnh.

## Giới hạn

- Kết quả kiểm thử không phải cam kết hệ thống không còn bất kỳ lỗi nào. Luồng người dùng thật, mạng/proxy production, OAuth và giao dịch thật cần smoke test sau triển khai.
- Không tự thay các chính sách chưa chốt: tính tiền vào/ra giữa tháng, nhiều hợp đồng trong cùng một kỳ và giữ chỗ hợp đồng tương lai.
- Các cảnh báo bundle lớn/annotation của dependency SignalR và lint ở file khác ngoài phạm vi vẫn cần kế hoạch tối ưu riêng; không đổi dependency hay cơ chế thanh toán để làm mất cảnh báo.

Để chạy lại fixture UI, chép artifacts/audit-ui/preview.html vào NovaStay/.codex-audit-preview.html, chạy Vite ở localhost:5184 rồi chạy check.cjs. Xóa file HTML tạm sau kiểm tra, không đưa vào bản triển khai.
