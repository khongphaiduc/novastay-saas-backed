# API phân trang cho các màn quản lý

Chỉ thay đổi backend. Không thay đổi frontend hoặc cấu trúc response của endpoint trả mảng cũ. Các endpoint mới có hậu tố `/paged` để frontend có thể tích hợp sau mà không làm sai thống kê hoặc mất dữ liệu của màn hình hiện tại.

## Tham số chung

- `pageIndex`: trang bắt đầu từ 1, mặc định 1.
- `pageSize`: mặc định 10, từ 1 đến 1000. Giới hạn giữ tương thích với các request thu chi hiện dùng `pageSize=1000`.
- Tham số ngoài phạm vi hoặc offset tràn số trả HTTP 400.
- Trang vượt quá dữ liệu trả `items: []`, vẫn giữ `totalCount` đúng theo bộ lọc.
- Count và Skip/Take thực hiện trong database, sau lọc; sắp xếp có khóa ID để các bản ghi cùng thời điểm/tên không trùng trang do thứ tự không xác định.
- Endpoint mới yêu cầu JWT BusinessOwner và giới hạn dữ liệu trong tổ chức/nhà trọ thuộc quyền người dùng.

Ví dụ response:

```json
{
  "items": [],
  "totalCount": 25,
  "pageIndex": 4,
  "pageSize": 10,
  "totalPages": 3,
  "hasPreviousPage": true,
  "hasNextPage": false
}
```

## Các endpoint

Tất cả là GET; thêm `?pageIndex=1&pageSize=10` và các bộ lọc cần dùng.

| Endpoint | Bộ lọc bổ sung |
| --- | --- |
| `/api/organizations/{organizationId}/residents/paged` | `status`, `search` (tên/số điện thoại) |
| `/api/billing/invoices/paged` | `propertyId`, `status` (`Unpaid`, `AwaitingConfirmation`, `Paid`), `period` (`yyyy-MM`), `search` (mã hóa đơn/người nộp/phòng) |
| `/api/organizations/{organizationId}/properties/{propertyId}/income-receipts/paged` | `status`, `paymentMethod` (enum hiện có), `search` (mã phiếu/người nộp/tham chiếu) |
| `/api/organizations/{organizationId}/properties/{propertyId}/expenses/paged` | `status`, `paymentMethod` (enum hiện có), `search` (mã phiếu/người nhận/tham chiếu) |
| `/api/utility-meters/by-property/{propertyId}/paged` | `search` (phòng/tên dịch vụ/mã đồng hồ) |
| `/api/utility-meters/by-room/{roomId}/paged` | `search` |
| `/api/utility-meters/{meterId}/readings/paged` | `billingPeriod` (`yyyy-MM`) |

Danh sách đồng hồ chỉ kèm chỉ số gần nhất: `latestReading` và tối đa một phần tử `readings`. Lấy lịch sử bằng endpoint `readings/paged`, không tải toàn bộ lịch sử theo mỗi đồng hồ trong danh sách.

API nhật ký tổng hợp `/api/organizations/{organizationId}/properties/{propertyId}/financial-transactions` đã có phân trang, tiếp tục dùng `pageIndex/pageSize` cùng các bộ lọc hiện tại (`transactionType`, `search`, `status`, `paymentMethod`, `fromDate`, `toDate`). Đã bổ sung kiểm tra giới hạn và thứ tự ổn định.

API cư dân cũ `/api/organizations/{organizationId}/residents` vốn phân trang ở repository bằng `page/pageSize` nhưng controller chỉ trả mảng, không trả tổng số. Endpoint `/residents/paged` mới trả metadata đầy đủ và dùng `pageIndex/pageSize` thống nhất. Endpoint cũ giữ nguyên để tương thích.

Các tổng thu/chi/công nợ phải được tính trên toàn bộ dữ liệu phù hợp, không cộng riêng `items` của một trang. Backend không chuyển các request frontend hiện có sang endpoint mới một cách âm thầm.

Phân trang không yêu cầu migration mới. Khởi động lại backend để dùng các route mới.
