# Vận hành PayOS demo

## Những gì đã triển khai

- Backend API PayOS và hub `/hubs/payments`, luồng Manual/PayOS tách theo hóa đơn.
- Chỉ owner demo có quyền chọn PayOS. Tài khoản khác nhận `requiresUpgrade=true`; chưa có chức năng mua/cấp gói PRO hoặc kết nối PayOS nhiều chủ trọ.
- Webhook kiểm tra HMAC SHA-256, orderCode, linkId, accountNumber, VND, amount và tổ chức sở hữu trước khi ghi nhận tiền.
- Một lần lưu database cập nhật hóa đơn, attempt, phiếu thực thu, sổ thu chi và notification outbox. Rowversion và các unique index chặn thanh toán trùng/concurrent.
- Gateway tạo link qua API chính thức, kiểm tra chữ ký response. Không thêm SDK hay API key vào frontend. Giao dịch chưa được kiểm chứng bằng chuyển khoản thật trong quá trình code.
- Worker đối soát khoảng 60 giây, tối đa 25 attempt mỗi lượt; outbox SignalR dispatch khoảng 5 giây, retry khi lỗi. HTTP GET status đọc database nên vẫn dùng khi SignalR ngắt kết nối.

## Cấu hình riêng trên server

Tham khảo file `payos-demo.env.example`, điền vào file môi trường đã ignore hoặc secret store của deployment. Không điền giá trị thật vào file example hoặc Git.

| Biến | Ý nghĩa |
| --- | --- |
| `PayOS__Enabled` | Đặt `true` sau khi đã cấu hình đầy đủ |
| `PayOS__DemoOwnerAccountId` | Owner demo, mặc định `5c695542-8920-4c39-a470-62d1452a2d2e` |
| `PayOS__ClientId`, `PayOS__ApiKey`, `PayOS__ChecksumKey` | Cùng một kênh PayOS đã liên kết ngân hàng |
| `PayOS__ExpectedAccountNumber` | Số tài khoản chính xác của kênh PayOS, không có khoảng trắng |
| `PayOS__VirtualAccountPrefix`, `PayOS__ExpectedBankBin` | Bật nhận tài khoản ảo động bằng tiền tố và BIN của ngân hàng thuộc kênh (ví dụ MB: `VQR`, `970422`). Mặc định để trống: chỉ nhận đúng ExpectedAccountNumber. Với VA, backend chỉ nhận tài khoản từ response có HMAC hợp lệ, đúng BIN/tiền tố, rồi lưu tài khoản cụ thể theo từng đơn; không dùng một số VA cố định cho mọi hóa đơn. Giữ ExpectedAccountNumber là tài khoản ngân hàng thật để đối chiếu webhook. |
| `PayOS__ReturnUrl`, `PayOS__CancelUrl` | Route frontend Antigravity sẽ tạo; HTTPS, HTTP chỉ được dùng với loopback local |
| `PayOS__LinkLifetimeMinutes` | Mặc định 30, giới hạn 5–1440 phút |

`ReturnUrl`/`CancelUrl` tự được thêm `invoiceId`; không đưa sẵn invoiceId trong cấu hình. Khởi động lại API để nhận thay đổi cấu hình. Bật cờ mà thiếu khóa/URL/tài khoản hợp lệ vẫn không cho tạo PayOS invoice.

Khi chạy `Development`, nếu `.env` local có cấu hình PayOS thì backend ưu tiên toàn bộ cấu hình PayOS từ file này, tránh biến môi trường cũ được IDE kế thừa làm trộn khóa của hai kênh. Khóa thiếu trong file không lấy bù từ Windows. Các biến ngoài PayOS và môi trường Production giữ nguyên độ ưu tiên hiện tại. Sau khi sửa code hoặc `.env` vẫn phải dừng và chạy lại backend.

Khóa PayOS phải thuộc kênh nhận tiền dành cho đúng chủ trọ demo. Không dùng một kênh để thu hộ tự động cho owner khác. Không đổi DemoOwner hoặc kênh sang một người khác khi còn link đang chờ; dữ liệu attempt đã gắn với chủ kênh lúc tạo. Đổi ảnh QR/tài khoản thủ công không đổi kênh PayOS.

## Migration và khởi động

Migration mới: `20261022050534_AddPayOsDemoPayments` (sau `AddManualTransferBilling`). Thêm PaymentMode/Source/Reference, bảng PayOsPaymentAttempts và PaymentNotificationOutbox; hóa đơn cũ nhận mode Manual. Không seed tài khoản, mật khẩu hay sửa gói thuê bao.

API hiện tự `Database.MigrateAsync()` khi khởi động. Trước khi deploy kiểm tra database đích và backup theo quy trình đang dùng. Chạy migration trên cùng bản code backend mới; frontend cũ vẫn dùng Manual mặc định. Không dùng file SQL cũ được sinh ở các lần kiểm tra trước để thay migration mới.

## Webhook

Đăng ký tại kênh PayOS:

```text
https://api.nestone.io.vn/api/payments/payos/webhook
```

Endpoint không yêu cầu JWT, chỉ chấp nhận dữ liệu có chữ ký đúng. Không thêm Authorization header từ frontend. Có thể đăng ký trên giao diện PayOS hoặc API `POST https://api-merchant.payos.vn/confirm-webhook` với x-client-id/x-api-key và body `{ "webhookUrl": "..." }` từ công cụ riêng của người vận hành.

PayOS gửi dữ liệu kiểm tra khi đăng ký webhook. Webhook có chữ ký hợp lệ nhưng orderCode không thuộc hệ thống được trả 200 với `matched=false`; không tạo phiếu hay thu tiền cho mẫu kiểm tra. Chữ ký sai trả 400. Lỗi database/concurrent tạm thời trả 503 để bên gửi có thể retry; gửi lặp sau thành công không tạo thêm khoản thu.

Localhost không nhận được webhook từ PayOS qua SSH tunnel database. Khi thử local cần HTTPS tunnel cho cổng API hoặc dùng API đã deploy trên VPS; điều chỉnh URL đăng ký webhook tới đúng môi trường. Không dùng dữ liệu chuyển khoản production để thử vào database khác.

Nếu thử trên local cổng HTTPS 7286, chạy `ngrok http https://localhost:7286 --host-header=localhost:7286`, rồi đăng ký `<public-url>/api/payments/payos/webhook` của tunnel hiện tại trong kênh PayOS. Giữ backend và ngrok chạy trong suốt lúc thử. SignalR phía trình duyệt vẫn kết nối backend local theo VITE_API_URL; không cần đổi frontend sang ngrok. Một kênh demo chỉ dùng một webhook đích: trước khi chạy trên VPS phải đăng ký lại URL webhook VPS. Không bật pooling giữa local và VPS vì sự kiện có thể tới một instance khác với instance đang giữ kết nối SignalR. Nếu chỉ worker đối soát nhận tiền, trạng thái có thể chậm khoảng 60 giây; webhook thành công cập nhật database ngay, outbox phát SignalR trong khoảng 5 giây. SentAt của outbox chỉ xác nhận đã gọi phát sự kiện, không chứng minh trình duyệt nhận hoặc hiển thị sự kiện.

Không chạy đồng thời API local và API VPS với cùng database khi kiểm tra realtime trong kiến trúc một instance hiện tại. Cả hai worker có thể đọc cùng outbox; instance nào cập nhật SentAt trước sẽ phát qua hub riêng của instance đó. Không có backplane nên trình duyệt nối instance còn lại không nhận được event dù Invoice đã Paid. Khi thử local với DB chung, tạm dừng riêng API VPS; vẫn giữ SQL/Redis/RabbitMQ chạy. Khi chuyển lại VPS, dừng API local, đăng ký webhook VPS và cho trình duyệt kết nối API VPS. Muốn chạy nhiều instance lâu dài cần cơ chế phân phối thông báo/backplane chung, không chỉ dùng chung SQL.

Frontend khởi tạo và thay kết nối theo accountId/accountType, tự thử lại sau lỗi ban đầu và tải lại dữ liệu khi kết nối hoặc reconnect thành công. Chỉ event Paid có eventId mới được hiển thị popup; event gửi lặp được loại trùng trong phiên. Popup đặt trên modal khác và giữ tới khi đóng, tránh người dùng chuyển sang ứng dụng ngân hàng bị lỡ toast. Chủ trọ tải lại hóa đơn, danh sách thu chi và thống kê công nợ từ API sau event; frontend không tự đánh dấu Paid trước xác nhận server.

## Nginx, CORS và WebSocket

Cho proxy cả `/hubs/payments/negotiate` và `/hubs/payments` tới API. Ví dụ dùng port host 17286 (kiểm tra lại port deployment thực tế):

```nginx
location /hubs/payments {
    proxy_pass http://127.0.0.1:17286;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 120s;
    access_log off;
}
```

WebSocket/SSE JWT có thể nằm trong query `access_token`. Không ghi query đó trong Nginx, application access logs hoặc telemetry. Có thể thay `access_log off` bằng format chỉ ghi `$uri`, không ghi `$request`, `$request_uri` hay `$args`. Không đưa URL chứa JWT vào log frontend. JWT từ query chỉ được backend chấp nhận dưới `/hubs/payments`, không dùng để xác thực các API thường.

CORS dùng allowlist `AllowedOrigins` đang có. Frontend dùng Bearer và `withCredentials:false`, không cần bật wildcard origin hoặc cookie credentials. Thiết kế hiện phù hợp một API instance; nhiều instance cần SignalR backplane/shared delivery trước khi mở rộng. Outbox có thể phát event lặp; eventId ổn định, frontend deduplicate và luôn đọc trạng thái lại qua API.

## Hết hạn và cần đối soát

- Không tạo order mới chỉ vì HTTP create timeout. OrderCode được lưu trước khi gọi provider và dùng lại khi retry; nếu PayOS đã tạo thì đọc lại order để phục hồi.
- Khi provider xác nhận Expired/Cancelled và chưa nhận tiền mới cho cấp link thay thế. Hết thời gian local nhưng provider chưa terminal trả `PAYMENT_NOT_TERMINAL`.
- Trường hợp phục hồi bằng GET có thể không có chuỗi QR; checkoutUrl vẫn có để mở trang PayOS.
- Thiếu/thừa tiền hoặc danh tính không khớp chuyển attempt sang NeedsReview và chặn tạo link tiếp. Không có API tự gán Paid hoặc hoàn tiền. ProviderLinkMismatch vẫn chặn đối soát tự động. Với WebhookIdentityMismatch của đơn VA đã có link được xác minh, backend chỉ phục hồi khi GET PayOS có HMAC hợp lệ chứng minh đúng owner/kênh, orderCode, linkId, tổng tiền và các giao dịch ngân hàng nhận đủ tiền vào đúng tài khoản cấu hình, không có reference trùng hoặc VA đối ứng khác. Thiếu bằng chứng vẫn giữ NeedsReview.
- Nếu link cũ nhận tiền trễ, hóa đơn chỉ được ghi nhận một lần. Link thay thế được đánh dấu CancellationPending, worker cố hủy tại PayOS; nếu link khác cũng đã nhận tiền thì ghi cảnh báo InvoiceAlreadyPaid, không tạo phiếu thực thu thứ hai. Trường hợp này cần kiểm tra giao dịch thật để xử lý chênh lệch ngoài luồng tự động.
- Khi tắt `Enabled`, không cho phát hành/link mới; webhook và đối soát giao dịch đã có vẫn cần giữ khóa kênh gốc để tiếp nhận tiền đang về.

## Kiểm thử và tài liệu gốc

### Webhook VA chỉ chứa tài khoản ngân hàng thật

Một số giao dịch VA trả `accountNumber` là tài khoản ngân hàng thật và để trống `virtualAccountNumber`, dù QR dùng VA riêng cho từng đơn. Backend không so sánh hai số này như cùng một tài khoản. Với webhook có chữ ký hợp lệ, đúng owner, mã đơn, link, VND, số tiền và reference, backend đọc lại GET `/v2/payment-requests/{orderCode}`. Chỉ xác nhận khi response có HMAC hợp lệ, trạng thái PAID, nhận đủ tiền và giao dịch ngân hàng/reference khớp. Nếu PayOS còn PENDING hoặc không truy cập được, webhook trả 503 để retry, không biến lỗi tạm thời thành một cảnh báo NeedsReview cố định.

Owner có thể gọi lại `POST /api/billing/invoices/{id}/reconcile` cho hóa đơn đã bị WebhookIdentityMismatch trước bản sửa. Worker cũng dùng cùng điều kiện xác minh này. Không xóa attempt, không chuyển khoản lại và không sửa Invoice.Status bằng SQL. Bản sửa không cần migration. Log chẩn đoán chỉ ghi mã đơn, invoiceId, loại lỗi và các cờ so sánh; không ghi raw webhook, khóa, chữ ký hoặc số tài khoản đầy đủ.

Lỗi thực tế đã được tái hiện bằng phản hồi GET PayOS có chữ ký hợp lệ của đơn đã thanh toán: replay webhook được dựng từ các trường giao dịch và đối soát hóa đơn NeedsReview trong hai database InMemory riêng. Mỗi luồng tạo đúng một phiếu thu, một khoản thu và một outbox event. Kiểm chứng này không ghi database thật và không thay thế một lần kiểm tra webhook chuyển khoản thật qua môi trường deploy mới.

Kiểm thử tự động dùng fake gateway, signed HTTP response fixtures, database InMemory và Kestrel/SignalR localhost với JWT thử nghiệm. Có kiểm tra chữ ký theo vector mẫu chính thức của PayOS. Đã đọc giao dịch thực từ PayOS, thử tạo QR thực và đăng ký webhook trong các bước vận hành; các kiểm thử tự động không thực hiện chuyển khoản hay thay đổi webhook đang dùng.

PayOS không có sandbox riêng. Sau khi cấu hình đúng, người vận hành thực hiện một hóa đơn demo giá trị nhỏ và tự chuyển khoản, kiểm tra cả hai giao diện, khoản thu, công nợ, reference và việc reload vẫn Paid. Không coi redirect hoặc toast là bằng chứng duy nhất.

- API: https://payos.vn/docs/api/
- HMAC webhook: https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/
- Môi trường thử nghiệm: https://payos.vn/docs/moi-truong-test/
- SignalR JWT: https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz?view=aspnetcore-8.0
- Bàn giao frontend: `antigravity-payos-signalr-frontend.md`.
