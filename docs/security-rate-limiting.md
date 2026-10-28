# Bảo vệ đăng nhập và endpoint nhạy cảm

## Bản sửa đăng nhập Google

Repository tái sử dụng model EF đã được theo dõi theo khóa chính khi Update/Remove, thay vì attach thêm một instance cùng Id. Luồng liên kết Google chỉ cập nhật Account một lần. Hai callback cùng danh tính Google được xử lý lần lượt trong một API instance; nếu nhiều API instance cùng tạo tài khoản, lỗi unique SQL Server được đọc lại và thử một lần với tài khoản thắng cuộc. Không xóa tài khoản hoặc đổi schema.

Token Google phải đúng ClientId, có Subject và email đã xác minh. Không ghi đè GoogleId khác đã liên kết hoặc cho tài khoản bị khóa đăng nhập. Frontend chặn callback Google lặp khi request trước chưa hoàn tất.

## Giới hạn mặc định

Các endpoint cùng policy chia sẻ ngân sách, dùng sliding window và trả ngay 429 khi hết lượt, không xếp hàng.

| Nhóm | Giới hạn | Phân biệt theo |
|---|---|---|
| Đăng nhập mật khẩu/Google | 30/phút | IP |
| Đăng ký | 10/giờ | IP |
| Quên mật khẩu | 5/15 phút | IP |
| Refresh token | 60/phút | IP |
| Đổi mật khẩu | 5/15 phút | AccountId |
| Tạo/lấy payment link | 10/phút | AccountId |
| Đối soát thanh toán | 6/phút | AccountId |
| Lưu tài khoản nhận tiền | 5/phút | AccountId |
| Gửi thông báo/mời cư dân/tạo link mời | 10/10 phút | AccountId |
| Xem/nhận lời mời | 20/phút | AccountId hoặc IP |
| Kết nối hub thanh toán | 30/phút | AccountId hoặc IP |

Lớp bổ sung theo đối tượng:

- Đăng nhập mật khẩu: tối đa 5 lần sai/5 phút cho cặp IP + email/số điện thoại; request đang chạy cũng chiếm lượt. Đăng nhập thành công xóa số lần sai của cặp đó.
- Quên mật khẩu: mỗi email tối đa 5/giờ, cách nhau 120 giây. Email có/không có tài khoản đều nhận cùng thông báo 202.
- Refresh: mỗi refresh token tối đa 30/phút.
- Payment link: mỗi người + hóa đơn cách nhau 3 giây; đối soát cách nhau 5 giây.
- Gửi nhắc gia hạn: mỗi người + hợp đồng cách nhau 60 giây.

Webhook PayOS không dùng ngân sách đăng nhập; cơ chế kiểm tra chữ ký/danh tính kênh và chống xử lý trùng của thanh toán được giữ nguyên. Giới hạn hub áp dụng request kết nối/negotiate, không giới hạn từng thông báo trong WebSocket.

429 có header `Retry-After` và JSON `code`, `message`, `retryAfterSeconds`. CORS cho phép đọc header này. Frontend giữ phiên khi refresh gặp 429, lỗi mạng hoặc 5xx; chỉ xóa phiên khi refresh bị 401/403. Các request refresh đồng thời cùng chờ một kết quả và cùng nhận lỗi khi thất bại.

## Cấu hình production sau Nginx/Docker

Trước khi triển khai, khai báo đúng IP proxy mà API nhìn thấy; nếu bỏ qua bước này, nhiều người có thể bị tính chung IP Docker gateway. Chỉ tin một hop và IP proxy cụ thể, không nhận X-Forwarded-For tùy ý từ Internet.

1. Xem network của container bằng `docker inspect novastay-api --format '{{json .NetworkSettings.Networks}}'`.
2. Dùng tên network thực tế: `docker network inspect TEN_NETWORK --format '{{range .IPAM.Config}}{{.Gateway}}{{end}}'`.
3. Với cấu hình host Nginx -> port Docker localhost hiện tại, khai báo IP gateway phù hợp trong file môi trường riêng: `ReverseProxy__KnownProxies__0=IP_GATEWAY_THUC_TE`. Không sao chép một IP ví dụ khi chưa kiểm tra network.
4. Nginx cần gửi X-Forwarded-For và X-Forwarded-Proto. Giữ port API chỉ bind localhost. Nếu có thêm CDN/proxy, phải thiết kế lại chuỗi proxy tin cậy trước khi dùng.
5. Nếu compose chỉ liệt kê từng biến dưới `environment`, thêm biến proxy và biến giới hạn vào đó hoặc dùng `env_file` phù hợp. `docker compose --env-file` phục vụ nội suy và không tự truyền mọi biến vào container. Tạo lại container sau khi đổi cấu hình.
6. Thử vài request giới hạn từ hai mạng khác nhau và xác nhận không bị gộp IP. Log 429 có policy + TraceId, không ghi mật khẩu/token.

Tên biến có thể xem trong `security-rate-limiting.env.example`; đây chỉ là cấu hình không chứa secret. Giá trị mặc định đã có trong code, không cần sao chép toàn bộ. Giới hạn theo đối tượng nêu trên hiện cố định trong code.

## Phạm vi và kiểm thử

Bộ đếm lưu trong RAM của mỗi API instance, mất khi restart. Phù hợp một instance hiện tại; khi chạy nhiều replica cần bộ đếm dùng chung (ví dụ Redis) để có giới hạn tổng. Khóa email/điện thoại/refresh token trong bộ đếm dùng HMAC, không lưu trực tiếp nội dung nhận diện. Rate limiting không thay thế phân quyền hoặc unique constraint của database.

Lỗi hệ thống không trả nguyên exception EF ra giao diện; log nội bộ giữ chi tiết cùng TraceId. Chưa thay đổi toàn bộ quy trình cấp lại mật khẩu, chưa bổ sung CAPTCHA hoặc chống bot phân tán.

Kiểm thử dùng database InMemory và HTTP server riêng: đăng nhập mới/lặp/song song, bảo toàn trạng thái Added của EF, IP chung/giả header, cooldown email và hóa đơn, phân vùng tài khoản, webhook không bị chặn bởi policy đăng nhập, thông báo lỗi an toàn. Nhánh xử lý unique SQL Server giữa nhiều instance chưa có bài kiểm thử với SQL Server thật.
