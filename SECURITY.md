# Cấu hình secret và xử lý sự cố

Không lưu mật khẩu, JWT signing key, SMTP App Password hoặc private key trong Git.
Các file `.env` và `.env.*` là cấu hình riêng; chỉ commit `.env.example`/`.env.sample` với giá trị secret để trống.

## Chạy local

Sao chép `NovaStay/NovaStay.Infrastructure/Config/.env.sample` sang `.env` trong cùng thư mục.
Điền cấu hình riêng. Tạo JWT signing key ngẫu nhiên ít nhất 32 byte; không dùng khóa ví dụ.
Ứng dụng nhận cấu hình `JWT__SecretKey`, `Redis__ConnectionString`, `MinIO__SecretKey`,
`SMTP__Password` qua biến môi trường; tên cũ dạng `JWT_SecretKey` vẫn được hỗ trợ.
Không đặt đồng thời cả tên cũ và tên mới với giá trị khác nhau.

## Deploy

Sao chép `deploy/.env.example` sang `/opt/novastay/env/.env` trên VPS và điền giá trị riêng.
Giới hạn quyền file bằng `chmod 600 /opt/novastay/env/.env`.
Chạy Compose với `--env-file /opt/novastay/env/.env`; không in `docker compose config`
hoặc `docker inspect` ra log công khai vì chúng có thể chứa secret.

Với `NovaStay/docker-compose.prod.yml`, sao chép `NovaStay/.env.example` sang
`NovaStay/.env`, điền cấu hình cho đúng mạng Docker rồi dùng `--env-file NovaStay/.env`.
`SQLString` phải trỏ đến SQL Server tương ứng với bộ Compose đang dùng.

Đổi biến môi trường không tự đổi mật khẩu trong volume SQL Server/RabbitMQ đang có dữ liệu.
Phải đổi mật khẩu trong dịch vụ, cập nhật cấu hình và tạo lại container.
Đổi JWT signing key cần đi kèm thu hồi refresh token để yêu cầu người dùng đăng nhập lại.
MinIO chỉ cần cập nhật dịch vụ nếu thực sự được triển khai; cấu hình dự phòng không phải dịch vụ đang chạy.

## Khi secret bị lộ

1. Thu hồi/đổi secret tại nhà cung cấp hoặc dịch vụ trước.
2. Cập nhật cấu hình riêng và xác minh secret cũ không còn đăng nhập được.
3. Kiểm tra log truy cập, thư đã gửi và thay đổi dữ liệu bất thường.
4. Làm sạch mã nguồn và, nếu cần, lịch sử tất cả nhánh/tag bằng `git-filter-repo`.
5. Sau khi viết lại lịch sử, các thành viên cần clone lại; không push nhánh từ clone cũ.

Không commit `.security-private/`: thư mục này chứa bản sao cấu hình cũ và dữ liệu khôi phục.
Docker build cũng phải loại `.env`, file cấu hình riêng và private key khỏi build context.
Nếu đã build image khi secret còn trong source, thay secret trước rồi build lại;
kiểm tra registry và cache build để xử lý các bản image có chứa secret.

Tài liệu: https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository
