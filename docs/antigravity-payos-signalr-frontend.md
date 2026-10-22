# Bàn giao Antigravity: PayOS demo và SignalR thanh toán

## Mục tiêu và phạm vi

Tích hợp frontend React hiện tại với API đã có. Giữ nguyên luồng chuyển khoản thủ công, cấu hình ngân hàng và ảnh QR tải lên. Bổ sung lựa chọn PayOS cho chủ trọ demo; các chủ trọ khác thấy nút **Nâng cấp PRO**. Backend hiện chỉ cấp tính năng tự động cho tài khoản demo đã cấu hình, chưa triển khai mua gói PRO hoặc kết nối PayOS cho chủ trọ khác.

Không hardcode Account ID để quyết định quyền trên frontend. Backend mặc định nhận diện tài khoản demo `5c695542-8920-4c39-a470-62d1452a2d2e`; quyền thực tế lấy từ API. Không yêu cầu hoặc lưu Client ID/API Key/Checksum Key trong frontend. Không dùng thông tin đăng nhập hoặc dữ liệu tài khoản đã được gửi trong hội thoại để seed mã nguồn.

Các file frontend liên quan hiện có:

- `NovaStay/src/BoardingHouseModule/BillingManagement.jsx`: cấu hình nhận tiền, phát hành và danh sách hóa đơn.
- `NovaStay/src/components/BillingCards.jsx`: hóa đơn người thuê/chủ trọ và ảnh QR thủ công.
- `NovaStay/src/BoardingHouseModule/AccountingManagement.jsx`: thống kê thu chi/công nợ và sổ giao dịch.
- `NovaStay/src/ResidentsModule/ResidentDashboard.jsx`: tab hóa đơn người thuê.
- `NovaStay/src/api/billingApi.js`: HTTP helper, JWT và lỗi API.
- `NovaStay/src/utils/auth.js`: `isTokenExpired()` và `refreshToken()` đang có sẵn.

## 1. Phân biệt hai phương thức

| Trường hợp | QR/tài khoản | Xác nhận |
| --- | --- | --- |
| `paymentMode = "Manual"` | `bankAccount` và ảnh do chủ trọ tải lên, giữ tính năng tải ảnh | Người thuê báo đã chuyển; chủ trọ xác nhận nhận tiền |
| `paymentMode = "PayOS"` | Lấy từ API payment-link, QR riêng theo hóa đơn | Webhook/đối soát server xác nhận; frontend không tự gán Paid |

Hóa đơn cũ mặc định Manual. Không chuyển phương thức của hóa đơn đã phát hành. Với PayOS, không hiển thị QR thủ công hoặc nút xác nhận thủ công/báo đã chuyển khoản để tránh chuyển nhầm tài khoản. Backend cũng chặn các thao tác thủ công đối với hóa đơn PayOS chưa thanh toán.

Ảnh QR thủ công và QR PayOS không thay thế cấu hình của nhau. QR PayOS gắn với ngân hàng đã liên kết tại PayOS; cấu hình ảnh QR thủ công không thay đổi tài khoản này.

## 2. Xác thực và API

Base URL dùng `import.meta.env.VITE_API_URL`, ví dụ production `https://api.nestone.io.vn`. Các API dưới đây yêu cầu `Authorization: Bearer <accessToken>`, trừ webhook dành riêng cho PayOS. Không gọi webhook từ frontend.

### Quyền tính năng — BusinessOwner

`GET /api/billing/payment-capabilities`

```json
{
  "isDemoAccount": true,
  "canUsePayOs": true,
  "requiresUpgrade": false,
  "configurationRequired": false,
  "reason": "DEMO_ENABLED",
  "availableModes": ["Manual", "PayOS"]
}
```

- `canUsePayOs=true`: cho chọn PayOS khi phát hành hóa đơn.
- `requiresUpgrade=true`: hiển thị nút **Nâng cấp PRO**, vẫn cho dùng Manual. Nút mở màn giới thiệu/đăng ký nhu cầu; không tự cấp quyền hoặc gọi API mua gói chưa tồn tại.
- `configurationRequired=true`: thông báo chủ trọ demo chưa được cấu hình PayOS trên server; vẫn dùng Manual.
- `reason`: `DEMO_ENABLED`, `PRO_UPGRADE_REQUIRED` hoặc `PAYOS_CONFIGURATION_REQUIRED`.
- Người thuê không gọi API này để kiểm tra gói của chính mình. Người thuê xác định phương thức từ hóa đơn được chủ trọ phát hành.

### Phát hành hóa đơn — BusinessOwner

`POST /api/billing/invoices`

Giữ request hiện tại, thêm `paymentMode`. Bỏ qua trường này tương đương Manual.

```json
{
  "propertyId": "<property-guid>",
  "period": "2026-10",
  "dueDate": "2026-10-31",
  "roomIds": ["<room-guid>"],
  "chargeKeys": ["rent", "service:<service-guid>"],
  "paymentMode": "PayOS"
}
```

Backend vẫn tự tính tiền, xác định người đại diện và chống thu trùng như trước. PayOS không yêu cầu có tài khoản QR thủ công; Manual vẫn yêu cầu cấu hình đó. Endpoint phát hành chưa gọi PayOS, nên lỗi mạng PayOS không làm mất đợt lập hóa đơn.

Response vẫn là danh sách hóa đơn của nhà trọ, không chỉ các hóa đơn vừa tạo. Mỗi `BillDto` bổ sung:

```json
{ "id": "<invoice-guid>", "paymentMode": "PayOS", "paymentSource": null }
```

`paymentSource` sau xác nhận là `Manual` hoặc `PayOS`; hóa đơn lịch sử có thể null. Danh sách owner/resident và API `/paged` vẫn dùng được.

### Tạo hoặc lấy lại link — BusinessOwner sở hữu / Resident người đại diện

`POST /api/billing/invoices/{invoiceId}/payment-link`

Không gửi số tiền, mã đơn, URL trả về hoặc thông tin ngân hàng. Backend xác định tất cả từ hóa đơn và cấu hình server.

Gọi khi mở chi tiết hóa đơn PayOS chưa trả hoặc khi người thuê bấm **Thanh toán PayOS**. Không gọi từ render hoặc vòng lặp không kiểm soát. Nhiều lần gọi sẽ dùng lại link đang Pending; mỗi hóa đơn chỉ có một attempt active.

Response là trạng thái đầy đủ:

```json
{
  "invoiceId": "<invoice-guid>",
  "paymentMode": "PayOS",
  "invoiceStatus": "Unpaid",
  "amount": 64000,
  "paymentSource": null,
  "paidAt": null,
  "payment": {
    "attemptId": "<attempt-guid>",
    "orderCode": 123456789,
    "status": "Pending",
    "paymentLinkId": "<provider-link-id>",
    "checkoutUrl": "https://pay.payos.vn/web/<provider-link-id>",
    "qrCode": "<chuỗi EMV do PayOS trả về>",
    "accountNumber": "<tài khoản đã liên kết PayOS>",
    "accountName": "<tên chủ tài khoản hoặc null>",
    "bankBin": "<mã ngân hàng hoặc null>",
    "expiresAt": "2026-10-22T05:30:00Z",
    "reviewReason": null
  }
}
```

**`qrCode` là nội dung QR dạng chuỗi, không phải URL ảnh.** Dùng thư viện render QR trên frontend; tạo PNG từ QR để tải xuống nếu cần. Có nút mở `checkoutUrl` với `rel="noopener noreferrer"`. Nếu link được khôi phục sau timeout, `qrCode` có thể null nhưng `checkoutUrl` vẫn dùng được: hiển thị nút mở PayOS thay vì ảnh lỗi. Dùng số tiền `amount` ở cấp ngoài; không tự tính lại thành tiền điện/nước tại đây.

### Đọc trạng thái — owner / người đại diện

`GET /api/billing/invoices/{invoiceId}/payment-status`

Trả cùng cấu trúc trên, chỉ đọc database; không gọi PayOS trực tiếp. `payment` có thể null khi chưa tạo link hoặc hóa đơn Manual.

### Đối soát — chỉ BusinessOwner sở hữu

`POST /api/billing/invoices/{invoiceId}/reconcile`

Gọi khi chủ trọ bấm **Kiểm tra giao dịch PayOS**. Backend hỏi PayOS, cập nhật hóa đơn nếu đủ điều kiện, trả trạng thái đầy đủ. Người thuê không được gọi endpoint này. Server cũng tự đối soát tối đa 25 attempt mỗi vòng khoảng 60 giây để khôi phục webhook bị bỏ lỡ.

## 3. Trạng thái giao diện

Luôn ưu tiên `invoiceStatus`; `payment.status` là trạng thái của một lần tạo link, không phải trạng thái toàn bộ hóa đơn.

| Giá trị | UI |
| --- | --- |
| `invoiceStatus=Paid` | Ẩn mọi QR/nút trả tiền, hiển thị đã thanh toán và tải lại thu chi/công nợ |
| `payment=null` | Hóa đơn PayOS: cho tạo link; Manual: dùng giao diện cũ |
| `payment.status=Creating` | Đang tạo/khôi phục link, cho tải lại trạng thái hoặc thử lại có giới hạn |
| `Pending` và chưa hết `expiresAt` | Hiện QR/link, thông tin nhận tiền và đếm thời gian |
| `Pending` nhưng đã qua `expiresAt` | Ẩn QR hết hạn, gọi tạo lại khi người dùng yêu cầu; có thể cần đợi PayOS xác nhận hết hạn |
| `Expired` / `Cancelled`, hóa đơn chưa trả | Cho yêu cầu link mới qua cùng API, không tự sinh mã đơn |
| `NeedsReview` | Ẩn QR, báo cần chủ trọ kiểm tra; không yêu cầu chuyển thêm tiền |
| `CancellationPending` | Link khác đang được hủy do hóa đơn đã được thanh toán bằng link trước; ưu tiên trạng thái hóa đơn |

`reviewReason` dùng cho owner: `AmountMismatch`, `WebhookIdentityMismatch`, `ProviderLinkMismatch`, `InvoiceAlreadyPaid`, `InvoiceNotPayable`. Không cung cấp nút tự gán Paid cho các trường hợp này. Giữ nút **Kiểm tra giao dịch PayOS** cho WebhookIdentityMismatch: backend có thể phục hồi đơn VA sau khi response GET có chữ ký hợp lệ chứng minh đúng đơn/link/kênh, số tiền và giao dịch ngân hàng. Không xóa cảnh báo hoặc gán Paid từ frontend. Hệ thống chưa có API hoàn tiền hoặc xử lý chênh lệch thủ công.

Return URL/cancel URL từ trình duyệt không xác nhận đã trả/hủy hóa đơn. Khi quay lại trang, dùng `invoiceId` trong query rồi gọi GET payment-status. Không tin `status`, `code`, `cancel` trên URL để cập nhật thu chi.

## 4. SignalR

Cài `@microsoft/signalr` trên frontend. Hub: `${API_ROOT}/hubs/payments` (không có `/api` phía trước). JWT dùng `accessTokenFactory`; dùng `withCredentials: false` vì app dùng Bearer, không dùng cookie.

Ví dụ module kết nối, điều chỉnh đường dẫn import theo vị trí file:

```js
import * as signalR from '@microsoft/signalr';
import { isTokenExpired, refreshToken } from '../utils/auth';

const API_ROOT = import.meta.env.VITE_API_URL || '';
const seenEvents = new Set();

async function token() {
  if (isTokenExpired()) return await refreshToken();
  return JSON.parse(localStorage.getItem('ns_account') || '{}').accessToken || '';
}

export function createPaymentConnection(reloadBillingData) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_ROOT}/hubs/payments`, {
      accessTokenFactory: token,
      withCredentials: false,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  connection.on('InvoicePaymentUpdated', async event => {
    if (seenEvents.has(event.eventId)) return;
    seenEvents.add(event.eventId);
    if (seenEvents.size > 1000) seenEvents.delete(seenEvents.values().next().value);
    await reloadBillingData(event.invoiceId); // API là nguồn dữ liệu chính.
    // Sau reload thành công mới hiển thị thông báo thanh toán thành công.
  });
  connection.onreconnected(() => reloadBillingData());
  return connection;
}
```

Event:

```json
{
  "eventId": "<outbox-guid>",
  "invoiceId": "<invoice-guid>",
  "status": "Paid",
  "paymentSource": "PayOS",
  "paidAt": "2026-10-22T05:00:00Z"
}
```

- Một connection theo phiên đăng nhập, không tạo riêng từng thẻ hóa đơn. Đăng ký handler trước `start()`, `stop()` và bỏ handler khi logout/unmount đúng cấp quản lý phiên.
- Server tự chọn owner và người đại diện. Không có phương thức JoinGroup hoặc ConfirmPayment để client gọi.
- `withAutomaticReconnect` không tự thử lại lần `start()` đầu thất bại và dừng sau chuỗi retry trên. UI cần xử lý lỗi start/onclose: có nút kết nối lại hoặc retry giới hạn khi người dùng vẫn đăng nhập. Vẫn cho tải trạng thái bằng HTTP.
- Token hết hạn làm hub đóng kết nối; reconnect đọc lại token mới. Logout dừng kết nối ngay, không tiếp tục retry refresh.
- Event có thể gửi lặp khi server retry; deduplicate theo `eventId`. Không dùng event để cộng trực tiếp thêm doanh thu; gọi API tải lại để tránh cộng hai lần.
- Outbox dispatch khoảng 5 giây. Nếu tab offline khi gửi, không đảm bảo nhận lại event; khi mở/quay lại tab hoặc reconnect phải tải lại hóa đơn và số liệu.
- Owner: tải lại danh sách hóa đơn, sổ giao dịch, số thực thu và công nợ. Resident: tải lại hóa đơn và ẩn QR đã trả.
- Xác nhận Manual cũng có thể phát event với `paymentSource="Manual"`; chỉ ghi “Thanh toán qua PayOS” khi source là PayOS.

## 5. Lỗi API cần xử lý

Response lỗi mới: `{ "code": "...", "message": "..." }`.

| HTTP / code | Xử lý |
| --- | --- |
| 401 / 403 | Refresh hoặc đăng nhập lại; không retry vòng lặp |
| 403 `PRO_UPGRADE_REQUIRED` | Hiện giới thiệu PRO, giữ Manual |
| 409 `PAYOS_CONFIGURATION_REQUIRED` | Báo cần cấu hình server |
| 409 `MANUAL_INVOICE` / `PAYOS_AUTOMATIC_CONFIRMATION` | Gọi sai luồng, điều chỉnh theo paymentMode |
| 409 `INVOICE_ALREADY_PAID` | GET lại trạng thái, ẩn nút thanh toán |
| 409 `INVOICE_NOT_PAYABLE` | Hóa đơn đã hủy/không còn được trả; ẩn nút và tải lại hóa đơn |
| 409 `PAYMENT_NEEDS_REVIEW` / `PAYOS_CHANNEL_MISMATCH` | Báo owner kiểm tra; không yêu cầu trả lại |
| 409 `PAYMENT_NOT_TERMINAL` | PayOS chưa xác nhận link cũ hết hạn; tải trạng thái, thử sau |
| 409 `PAYMENT_CONCURRENT_UPDATE` | GET trạng thái rồi mới retry có giới hạn |
| 503 `PAYOS_*` | Chưa xác định kết quả tạo/đối soát. Không tạo hóa đơn khác; GET trạng thái hoặc retry cùng invoiceId |
| 404 | Hóa đơn không tồn tại/không thuộc quyền; không lộ dữ liệu người khác |

Không tạo lại phiếu thu khi API tạo link gặp 503; cùng invoiceId được phục hồi bằng cùng orderCode đã lưu. Không tự fallback sang chuyển khoản tay khi PayOS đang có link: có thể phát sinh hai lần chuyển tiền.

## 6. Checklist nghiệm thu

1. Chủ trọ thường thấy nút PRO; chọn PayOS bằng request tự sửa bị backend chặn.
2. Demo thiếu cấu hình nhận thông báo phù hợp và vẫn dùng Manual được.
3. Demo đã cấu hình có thể phát hành PayOS theo phòng; người khác không xem được link.
4. Người thuê hiển thị đúng QR chuỗi và số tiền do server trả; QR thủ công vẫn hiển thị/tải được cho Manual.
5. Bấm tạo link nhiều lần không tạo nhiều order; timeout không tạo lại hóa đơn.
6. Sau thanh toán thật đã xác nhận, cả hai phía được cập nhật; reload vẫn thấy Paid.
7. Ngắt mạng SignalR rồi kết nối lại vẫn lấy đúng trạng thái bằng HTTP.
8. Event trùng không tạo toast/thu nhập trùng; webhook hoặc redirect không được gọi để giả lập Paid từ frontend.
9. Hết hạn, sai số tiền, NeedsReview và lỗi cấu hình có UI rõ ràng; không khuyến khích chuyển tiền lần hai.
10. Chạy luồng Manual như trước để kiểm tra hồi quy.

Backend đã có migration và unit/integration tests. Cấu hình PayOS thực, webhook HTTPS và Nginx WebSocket xem `payos-demo-operations.md`. Chưa có frontend PayOS trong lần thay đổi backend này.

## Prompt có thể giao Antigravity

“Hãy đọc toàn bộ tài liệu này và tích hợp frontend PayOS/SignalR vào NovaStay hiện tại. Giữ luồng Manual và cấu hình QR tải lên; dùng payment-capabilities để hiện quyền demo hoặc nút PRO. Dùng các API đã ghi, không tự thêm endpoint giả hoặc gán Paid ở client. Kết nối một SignalR hub theo phiên, refresh JWT/reconnect, tải lại dữ liệu sau event và khi quay về trang thanh toán. QR PayOS là chuỗi EMV cần render thành ảnh; không dùng làm URL img. Làm đầy đủ các trạng thái lỗi và checklist nghiệm thu. Không sửa backend hay đưa secret vào frontend.”
