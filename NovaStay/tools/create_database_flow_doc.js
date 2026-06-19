const fs = require("fs");
const path = require("path");
const childProcess = require("child_process");

const root = path.resolve(__dirname, "..");
const outDir = path.join(root, "docs");
const buildDir = path.join(root, ".tmp_docx_database_flow");
const outFile = path.join(outDir, "database-flow-host-operations.docx");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function cleanDir(dir) {
  fs.rmSync(dir, { recursive: true, force: true });
  ensureDir(dir);
}

function xmlEscape(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function attr(value) {
  return xmlEscape(value);
}

function textRuns(text, opts = {}) {
  const preserve = /^\s|\s$/.test(text) ? ' xml:space="preserve"' : "";
  const rPr = [
    opts.bold ? "<w:b/>" : "",
    opts.italic ? "<w:i/>" : "",
    opts.color ? `<w:color w:val="${attr(opts.color)}"/>` : "",
    opts.size ? `<w:sz w:val="${opts.size * 2}"/>` : "",
  ].join("");
  return `<w:r>${rPr ? `<w:rPr>${rPr}</w:rPr>` : ""}<w:t${preserve}>${xmlEscape(text)}</w:t></w:r>`;
}

function para(text = "", style = "Normal", opts = {}) {
  const pPr = [
    style ? `<w:pStyle w:val="${style}"/>` : "",
    opts.keepNext ? "<w:keepNext/>" : "",
    opts.align ? `<w:jc w:val="${opts.align}"/>` : "",
    opts.numId ? `<w:numPr><w:ilvl w:val="0"/><w:numId w:val="${opts.numId}"/></w:numPr>` : "",
    opts.shading ? `<w:shd w:fill="${opts.shading}"/>` : "",
    opts.before || opts.after || opts.line
      ? `<w:spacing${opts.before ? ` w:before="${opts.before}"` : ""}${opts.after ? ` w:after="${opts.after}"` : ""}${opts.line ? ` w:line="${opts.line}" w:lineRule="auto"` : ""}/>`
      : "",
  ].join("");
  return `<w:p>${pPr ? `<w:pPr>${pPr}</w:pPr>` : ""}${text ? textRuns(text, opts) : ""}</w:p>`;
}

function richPara(parts, style = "Normal", opts = {}) {
  const pPr = [
    style ? `<w:pStyle w:val="${style}"/>` : "",
    opts.keepNext ? "<w:keepNext/>" : "",
    opts.align ? `<w:jc w:val="${opts.align}"/>` : "",
  ].join("");
  return `<w:p>${pPr ? `<w:pPr>${pPr}</w:pPr>` : ""}${parts.map((part) => textRuns(part.text, part)).join("")}</w:p>`;
}

function bullet(text) {
  return para(text, "Normal", { numId: 1, line: 280, after: 120 });
}

function number(text) {
  return para(text, "Normal", { numId: 2, line: 280, after: 120 });
}

function cell(content, width, opts = {}) {
  const children = Array.isArray(content) ? content.join("") : para(content);
  const fill = opts.fill ? `<w:shd w:fill="${opts.fill}"/>` : "";
  const vAlign = `<w:vAlign w:val="${opts.vAlign || "center"}"/>`;
  return `<w:tc><w:tcPr><w:tcW w:w="${width}" w:type="dxa"/>${fill}${vAlign}<w:tcMar><w:top w:w="80" w:type="dxa"/><w:bottom w:w="80" w:type="dxa"/><w:start w:w="120" w:type="dxa"/><w:end w:w="120" w:type="dxa"/></w:tcMar></w:tcPr>${children}</w:tc>`;
}

function table(headers, rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0);
  const grid = widths.map((w) => `<w:gridCol w:w="${w}"/>`).join("");
  const headerRow = `<w:tr><w:trPr><w:tblHeader/></w:trPr>${headers
    .map((h, i) => cell([para(h, "TableHeader")], widths[i], { fill: "F2F4F7" }))
    .join("")}</w:tr>`;
  const bodyRows = rows
    .map((row) => `<w:tr>${row.map((c, i) => cell([para(c, "TableText")], widths[i])).join("")}</w:tr>`)
    .join("");
  return `<w:tbl><w:tblPr><w:tblStyle w:val="TableGrid"/><w:tblW w:w="${total}" w:type="dxa"/><w:tblInd w:w="120" w:type="dxa"/><w:tblLayout w:type="fixed"/><w:tblBorders><w:top w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/><w:left w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/><w:bottom w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/><w:right w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/><w:insideH w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/><w:insideV w:val="single" w:sz="4" w:space="0" w:color="D9DEE7"/></w:tblBorders><w:tblCellMar><w:top w:w="80" w:type="dxa"/><w:bottom w:w="80" w:type="dxa"/><w:start w:w="120" w:type="dxa"/><w:end w:w="120" w:type="dxa"/></w:tblCellMar></w:tblPr><w:tblGrid>${grid}</w:tblGrid>${headerRow}${bodyRows}</w:tbl>`;
}

function callout(title, body) {
  return table(["Khuyến nghị chính"], [[`${title}: ${body}`]], [9360]);
}

const tableInventory = [
  ["SubscriptionPackages", "Gói SaaS", "Lưu gói thuê bao, giá tháng, token tặng, quyền AI/custom role.", "Được Tenant tham chiếu qua PackageId."],
  ["Tenants", "Khách hàng B2B", "Mỗi đơn vị vận hành một chuỗi trọ/homestay/sleepbox. Là ranh giới multi-tenant.", "Gốc của hầu hết dữ liệu nghiệp vụ."],
  ["Users", "Tài khoản nhân sự", "Nhân viên/owner đăng nhập, thuộc một tenant, có thể được phân quyền theo property.", "Liên kết Role và Property qua bảng many-to-many."],
  ["Roles", "Vai trò", "Nhóm quyền như Owner, Manager, Receptionist, Technician.", "Thuộc Tenant; gắn Permission qua RolePermissions."],
  ["Permissions", "Quyền hệ thống", "Danh mục quyền theo module, ví dụ room.read, invoice.create.", "Dùng để kiểm soát RBAC."],
  ["Properties", "Cơ sở vận hành", "Một nhà trọ, homestay, tòa KTX, sleepbox site hoặc nhà nghỉ.", "Thuộc Tenant; chứa Rooms."],
  ["Rooms", "Phòng/đơn vị cho thuê", "Phòng thuê, phòng homestay, phòng nhà nghỉ; có giá cơ bản, trạng thái, sức chứa.", "Thuộc Property; là trung tâm vận hành."],
  ["RoomAvailability", "Lịch phòng", "Theo dõi ngày nào phòng đã được book, giá động theo ngày.", "Phục vụ booking ngắn hạn và homestay/nhà nghỉ."],
  ["RoomImages", "Ảnh phòng", "Ảnh mô tả phòng, ảnh cover, thứ tự hiển thị.", "Dùng cho listing và vận hành nội bộ."],
  ["Assets", "Danh mục tài sản", "Giường, TV, máy lạnh, tủ lạnh, bàn ghế, khóa, thiết bị PCCC.", "Không nằm trực tiếp trong phòng nếu chưa assign."],
  ["AssetAssignments", "Tài sản trong phòng", "Gắn tài sản vào phòng, ghi tình trạng, ghi chú, ngày bàn giao.", "Thay thế nhu cầu quản lý bảng Beds riêng."],
  ["Listings", "Tin đăng", "Thông tin public/marketplace của phòng: tiêu đề, mô tả, tiện nghi, publish.", "Gắn với Property và Room."],
  ["ListingImages", "Ảnh tin đăng", "Ảnh theo listing, thứ tự hiển thị.", "Tách khỏi RoomImages để tùy biến marketing."],
  ["Bookings", "Đặt phòng", "Lead/booking từ khách, check-in/out, số tiền, trạng thái thanh toán.", "Có thể sinh Contract và Invoice."],
  ["Residents", "Khách/người ở", "Hồ sơ cư dân/khách: định danh, ảnh giấy tờ, liên hệ.", "Dùng trong hợp đồng và bảo trì."],
  ["Contracts", "Hợp đồng", "Ràng buộc thuê phòng dài hạn, đặt cọc, môi giới, PDF hợp đồng, trạng thái.", "Liên kết Booking, Resident, Room."],
  ["Invoices", "Hóa đơn", "Tiền phòng, dịch vụ, tổng tiền, kỳ hóa đơn, QR, trạng thái thanh toán.", "Gắn Booking hoặc Contract."],
  ["Brokers", "Môi giới", "Thông tin môi giới, hoa hồng, ví hoa hồng.", "Có thể gắn với Contract."],
  ["MaintenanceTickets", "Phiếu bảo trì", "Sự cố phòng/tài sản, mô tả, ảnh trước/sau, kỹ thuật viên, trạng thái.", "Gắn Room, Resident, Technician, AssetAssignment."],
  ["Technicians", "Kỹ thuật viên", "Nhân sự xử lý bảo trì, chuyên môn, trạng thái sẵn sàng.", "Thuộc Tenant."],
  ["UserAccessTokens", "Access token", "Token đăng nhập ngắn hạn đã hash, hạn dùng, IP tạo/revoke.", "Tăng an toàn đăng nhập."],
  ["UserRefreshTokens", "Refresh token", "Token làm mới phiên đăng nhập, hash unique, chuỗi thay thế/revoke.", "Hỗ trợ session rotation."],
];

const improvements = [
  ["Chuẩn hóa trạng thái", "Tách status string thành enum/value constants hoặc lookup table cho Room, Contract, Invoice, Ticket.", "Giảm lỗi nhập sai như Available/available/Avaliable."],
  ["Bổ sung Services và MeterReadings", "Thêm dịch vụ điện, nước, internet, vệ sinh; thêm chỉ số công tơ theo phòng/kỳ.", "Cần cho nhà trọ dài hạn và sleepbox có phụ phí."],
  ["Tách PaymentTransactions", "Không chỉ lưu PaymentTransactionId trong Booking; nên có bảng giao dịch thanh toán riêng.", "Theo dõi nhiều lần thanh toán, hoàn tiền, thất bại, webhook."],
  ["Chuẩn hóa pricing", "Tách RoomRatePlan/SeasonalRate cho homestay/nhà nghỉ, giữ BasePrice cho giá mặc định.", "Hỗ trợ cuối tuần, lễ, theo giờ, theo ngày, theo tháng."],
  ["Audit log", "Thêm AuditLogs cho thay đổi hợp đồng, giá, hóa đơn, room status.", "Cần khi nhiều nhân sự cùng vận hành."],
  ["Soft delete", "Thêm IsDeleted/DeletedAt cho dữ liệu nghiệp vụ chính.", "Tránh mất lịch sử hóa đơn/hợp đồng khi xóa nhầm."],
  ["Ràng buộc tenant consistency", "Đảm bảo Room, Property, Booking, Contract cùng Tenant bằng validate ở application layer hoặc constraint phức hợp.", "Tránh cross-tenant data leak."],
  ["Chuẩn hóa tài sản trong phòng", "AssetAssignments nên có Quantity, Condition, SerialNumber, AssignedToResidentId nếu cần bàn giao cá nhân.", "Quản lý giường/TV/tủ như tài sản, đúng hướng bạn chọn."],
  ["Housekeeping", "Thêm bảng HousekeepingTasks cho homestay/nhà nghỉ: dọn phòng, kiểm phòng, linen.", "Quan trọng với lưu trú ngắn hạn."],
  ["Booking channel", "Thêm BookingSource/Channels: trực tiếp, Facebook, OTA, Booking.com, Agoda.", "Đo hiệu quả kênh bán và phí hoa hồng."],
];

const flows = [
  ["Onboarding tenant", "SubscriptionPackages -> Tenants -> Users/Roles/Permissions -> UserPropertyMapping."],
  ["Tạo cơ sở và phòng", "Tenants -> Properties -> Rooms -> RoomImages -> RoomAvailability."],
  ["Quản lý tài sản phòng", "Assets -> AssetAssignments -> MaintenanceTickets khi có sự cố."],
  ["Đăng bán/hiển thị phòng", "Rooms + RoomImages -> Listings -> ListingImages -> Booking."],
  ["Booking đến hợp đồng", "Bookings -> Residents -> Contracts -> Invoices."],
  ["Vận hành thu tiền", "Contracts/Bookings -> Invoices -> Payment status/QR -> đối soát."],
  ["Bảo trì", "Resident/Staff tạo MaintenanceTickets -> Technician xử lý -> cập nhật status/ảnh resolved."],
];

const roadmap = [
  ["Giai đoạn 1", "Ổn định core CRUD", "Chuẩn hóa status, thêm Quantity vào AssetAssignments, hoàn thiện Rooms/Properties/Bookings/Contracts/Invoices."],
  ["Giai đoạn 2", "Thu tiền và dịch vụ", "Thêm Services, MeterReadings, PaymentTransactions, invoice line items."],
  ["Giai đoạn 3", "Vận hành lưu trú ngắn hạn", "Rate plans, availability theo ngày/giờ, housekeeping, channel/source booking."],
  ["Giai đoạn 4", "Kiểm soát và mở rộng", "AuditLogs, soft delete, tenant consistency checks, reporting dashboard, notification."],
];

const body = [];
body.push(para("TÀI LIỆU PHÂN TÍCH DATABASE", "Title"));
body.push(para("Flow dữ liệu, công dụng bảng và hướng cải thiện cho phần mềm quản trị vận hành nhà trọ, KTX, sleepbox, homestay và nhà nghỉ", "Subtitle"));
body.push(richPara([
  { text: "Phạm vi: ", bold: true },
  { text: "Host.API / Host.Infrastructure sau migration RemoveBeds. Giường được quản lý như tài sản trong phòng qua Assets và AssetAssignments, không còn bảng Beds riêng." },
], "Meta"));
body.push(callout("Kết luận thiết kế", "Mô hình hiện tại nên lấy Room làm đơn vị vận hành chính. Các thiết bị như giường, TV, máy lạnh, tủ lạnh nên đi qua Assets và AssetAssignments. Cách này phù hợp với mục tiêu đơn giản hóa nhưng vẫn đủ mở rộng cho nhà trọ, homestay, sleepbox và nhà nghỉ."));

body.push(para("1. Tổng quan kiến trúc dữ liệu", "Heading1"));
body.push(para("Database hiện tại đi theo mô hình multi-tenant SaaS. Tenant là khách hàng sử dụng hệ thống; bên dưới tenant là các cơ sở vận hành, phòng, tài sản, booking, hợp đồng, hóa đơn, cư dân, bảo trì và phân quyền nhân sự."));
body.push(para("Các nhóm bảng chính", "Heading2"));
body.push(bullet("Nhóm SaaS và phân quyền: SubscriptionPackages, Tenants, Users, Roles, Permissions, UserAccessTokens, UserRefreshTokens."));
body.push(bullet("Nhóm cơ sở và phòng: Properties, Rooms, RoomAvailability, RoomImages."));
body.push(bullet("Nhóm tài sản: Assets, AssetAssignments."));
body.push(bullet("Nhóm bán phòng và khách: Listings, ListingImages, Bookings, Residents."));
body.push(bullet("Nhóm hợp đồng và tài chính: Contracts, Invoices, Brokers."));
body.push(bullet("Nhóm vận hành sau thuê: MaintenanceTickets, Technicians."));

body.push(para("2. Flow dữ liệu nghiệp vụ", "Heading1"));
body.push(table(["Flow", "Chuỗi dữ liệu"], flows, [2500, 6860]));
body.push(para("Nhìn tổng thể, Rooms là trung tâm của vận hành. Listing giúp bán phòng, Booking ghi nhận nhu cầu thuê, Contract xác lập thuê dài hạn, Invoice thu tiền, MaintenanceTicket xử lý sự cố. Assets và AssetAssignments giúp mô tả đồ đạc trong phòng mà không cần tạo bảng riêng cho từng loại đồ."));

body.push(para("3. Công dụng từng bảng", "Heading1"));
body.push(table(["Bảng", "Vai trò", "Công dụng", "Quan hệ chính"], tableInventory, [1900, 1550, 3500, 2410]));

body.push(para("4. Luồng vận hành đề xuất theo loại mô hình host", "Heading1"));
body.push(para("Nhà trọ dài hạn", "Heading2"));
body.push(number("Tạo Tenant, Property, Rooms và AssetAssignments cho từng phòng."));
body.push(number("Tạo Resident khi khách chốt thuê."));
body.push(number("Tạo Contract cho Room, ngày bắt đầu/kết thúc, đặt cọc, trạng thái."));
body.push(number("Sinh Invoice theo tháng; về sau nên có invoice line items cho điện, nước và dịch vụ."));
body.push(number("Khi có sự cố, tạo MaintenanceTicket gắn Room hoặc AssetAssignment."));

body.push(para("Homestay và nhà nghỉ", "Heading2"));
body.push(number("Tạo RoomAvailability theo ngày, có DynamicPrice."));
body.push(number("Listing hiển thị phòng và hình ảnh."));
body.push(number("Booking chứa check-in/check-out, amount và payment status."));
body.push(number("Invoice có thể gắn Booking nếu không cần hợp đồng dài hạn."));
body.push(number("Nên bổ sung housekeeping để quản lý dọn phòng sau check-out."));

body.push(para("Sleepbox/KTX theo hướng đơn giản", "Heading2"));
body.push(para("Nếu không bán từng giường riêng, vẫn giữ Room là đơn vị thuê. Số lượng giường, box, locker, TV, máy lạnh được quản lý bằng Assets và AssetAssignments. Ví dụ: Room A101 có AssetAssignment Giường x 4, Locker x 4, TV x 1. Nếu tương lai muốn bán từng box riêng, có thể thêm RentalUnits sau mà không phá Rooms."));

body.push(para("5. Đánh giá thiết kế hiện tại", "Heading1"));
body.push(para("Điểm tốt", "Heading2"));
body.push(bullet("Đã có ranh giới Tenant rõ, phù hợp SaaS nhiều chủ trọ/cơ sở."));
body.push(bullet("Có phân quyền Role/Permission và mapping user-property, phù hợp nhiều nhân sự vận hành."));
body.push(bullet("Room là trung tâm, phù hợp quyết định bỏ bảng Beds riêng."));
body.push(bullet("Có đủ trục nghiệp vụ cơ bản: listing, booking, contract, invoice, maintenance."));
body.push(bullet("Có refresh/access token riêng, tốt hơn lưu token thẳng trên User."));
body.push(para("Rủi ro cần xử lý sớm", "Heading2"));
body.push(bullet("Nhiều cột Status đang là string tự do, dễ phát sinh dữ liệu không thống nhất."));
body.push(bullet("Invoice hiện chưa có chi tiết dòng tiền điện/nước/dịch vụ, sẽ khó vận hành nhà trọ dài hạn."));
body.push(bullet("Booking/Contract/Invoice chưa có payment transaction riêng, khó đối soát nhiều lần thanh toán."));
body.push(bullet("AssetAssignments chưa có Quantity nên quản lý 'Giường x 4' sẽ phải tạo nhiều dòng hoặc ghi chú thủ công."));
body.push(bullet("Chưa thấy audit log/soft delete, rủi ro khi người dùng thao tác nhầm trong dữ liệu tài chính/hợp đồng."));

body.push(para("6. Đề xuất cải thiện schema", "Heading1"));
body.push(table(["Hạng mục", "Đề xuất", "Lý do"], improvements, [2100, 3900, 3360]));

body.push(para("7. Roadmap triển khai hợp lý", "Heading1"));
body.push(table(["Giai đoạn", "Mục tiêu", "Việc nên làm"], roadmap, [1600, 2400, 5360]));

body.push(para("8. Gợi ý schema bổ sung", "Heading1"));
body.push(para("Các bảng nên cân nhắc thêm khi sản phẩm đi vào vận hành thực tế:"));
body.push(bullet("Services: định nghĩa điện, nước, internet, gửi xe, vệ sinh, phụ thu."));
body.push(bullet("RoomServicePrices: giá dịch vụ theo phòng/cơ sở/thời điểm."));
body.push(bullet("MeterReadings: chỉ số điện/nước theo phòng và kỳ."));
body.push(bullet("InvoiceItems: chi tiết hóa đơn, gồm tiền phòng, điện, nước, dịch vụ, giảm giá, phụ phí."));
body.push(bullet("PaymentTransactions: giao dịch thanh toán, webhook, provider, trạng thái, amount, paidAt."));
body.push(bullet("HousekeepingTasks: dọn phòng, kiểm phòng, thay ga, trạng thái sau check-out."));
body.push(bullet("AuditLogs: ai thay đổi gì, lúc nào, trước/sau ra sao."));
body.push(bullet("Notifications: lịch nhắc thu tiền, hết hạn hợp đồng, check-in/check-out, ticket quá hạn."));

body.push(para("9. Quy tắc thiết kế nên giữ", "Heading1"));
body.push(bullet("Không tạo bảng riêng cho từng loại thiết bị như Bed, TV, AirConditioner. Dùng Assets + AssetAssignments."));
body.push(bullet("Chỉ tạo bảng riêng khi đối tượng có vòng đời nghiệp vụ riêng: được bán riêng, đặt riêng, định giá riêng, bảo trì riêng, báo cáo riêng."));
body.push(bullet("Giữ Room là đơn vị vận hành chính cho phiên bản đơn giản."));
body.push(bullet("Mọi dữ liệu nghiệp vụ quan trọng nên có TenantId trực tiếp hoặc kiểm soát được đường đi về Tenant."));
body.push(bullet("Các thao tác tài chính và hợp đồng nên ưu tiên auditability hơn là xóa cứng."));

body.push(para("10. Kết luận", "Heading1"));
body.push(para("Database hiện tại đã đủ nền cho MVP quản trị nhà trọ, homestay, sleepbox và nhà nghỉ nếu phạm vi ban đầu là quản lý theo phòng. Quyết định bỏ bảng Beds là hợp lý với mục tiêu đơn giản: giường trở thành tài sản trong phòng. Hướng cải thiện quan trọng nhất tiếp theo là chuẩn hóa status, thêm số lượng tài sản trong phòng, bổ sung dịch vụ/chỉ số công tơ/chi tiết hóa đơn và payment transaction để hệ thống vận hành được ngoài thực tế."));

const documentXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:wpc="http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:m="http://schemas.openxmlformats.org/officeDocument/2006/math" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:wp14="http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing" xmlns:w10="urn:schemas-microsoft-com:office:word" xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:w14="http://schemas.microsoft.com/office/word/2010/wordml" xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup" xmlns:wpi="http://schemas.microsoft.com/office/word/2010/wordprocessingInk" xmlns:wne="http://schemas.microsoft.com/office/word/2006/wordml" xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape" mc:Ignorable="w14 wp14"><w:body>${body.join("")}<w:sectPr><w:headerReference w:type="default" r:id="rIdHeader1"/><w:footerReference w:type="default" r:id="rIdFooter1"/><w:pgSz w:w="12240" w:h="15840"/><w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="708" w:footer="708" w:gutter="0"/><w:cols w:space="720"/><w:docGrid w:linePitch="360"/></w:sectPr></w:body></w:document>`;

const stylesXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="22"/><w:color w:val="1F2937"/></w:rPr></w:rPrDefault><w:pPrDefault><w:pPr><w:spacing w:after="120" w:line="264" w:lineRule="auto"/></w:pPr></w:pPrDefault></w:docDefaults>
<w:style w:type="paragraph" w:default="1" w:styleId="Normal"><w:name w:val="Normal"/><w:qFormat/><w:pPr><w:spacing w:after="120" w:line="264" w:lineRule="auto"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="22"/><w:color w:val="1F2937"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Title"><w:name w:val="Title"/><w:qFormat/><w:pPr><w:spacing w:before="0" w:after="80"/><w:keepNext/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:b/><w:sz w:val="52"/><w:color w:val="0B2545"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Subtitle"><w:name w:val="Subtitle"/><w:qFormat/><w:pPr><w:spacing w:after="240"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="28"/><w:color w:val="475569"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Meta"><w:name w:val="Meta"/><w:pPr><w:spacing w:after="160"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="20"/><w:color w:val="475569"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/><w:pPr><w:keepNext/><w:spacing w:before="320" w:after="160"/><w:outlineLvl w:val="0"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:b/><w:sz w:val="32"/><w:color w:val="2E74B5"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="heading 2"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/><w:pPr><w:keepNext/><w:spacing w:before="240" w:after="120"/><w:outlineLvl w:val="1"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:b/><w:sz w:val="26"/><w:color w:val="2E74B5"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="TableText"><w:name w:val="Table Text"/><w:pPr><w:spacing w:after="40" w:line="264" w:lineRule="auto"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:sz w:val="19"/><w:color w:val="1F2937"/></w:rPr></w:style>
<w:style w:type="paragraph" w:styleId="TableHeader"><w:name w:val="Table Header"/><w:pPr><w:spacing w:after="40"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri"/><w:b/><w:sz w:val="19"/><w:color w:val="0B2545"/></w:rPr></w:style>
</w:styles>`;

const numberingXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:numbering xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
<w:abstractNum w:abstractNumId="1"><w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="bullet"/><w:lvlText w:val="•"/><w:lvlJc w:val="left"/><w:pPr><w:tabs><w:tab w:val="num" w:pos="720"/></w:tabs><w:ind w:left="720" w:hanging="360"/><w:spacing w:after="160" w:line="280" w:lineRule="auto"/></w:pPr><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri" w:hint="default"/></w:rPr></w:lvl></w:abstractNum>
<w:abstractNum w:abstractNumId="2"><w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="decimal"/><w:lvlText w:val="%1."/><w:lvlJc w:val="left"/><w:pPr><w:tabs><w:tab w:val="num" w:pos="720"/></w:tabs><w:ind w:left="720" w:hanging="360"/><w:spacing w:after="160" w:line="280" w:lineRule="auto"/></w:pPr></w:lvl></w:abstractNum>
<w:num w:numId="1"><w:abstractNumId w:val="1"/></w:num>
<w:num w:numId="2"><w:abstractNumId w:val="2"/></w:num>
</w:numbering>`;

const documentRels = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rIdStyles" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
<Relationship Id="rIdNumbering" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>
<Relationship Id="rIdSettings" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings" Target="settings.xml"/>
<Relationship Id="rIdHeader1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/header" Target="header1.xml"/>
<Relationship Id="rIdFooter1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer1.xml"/>
</Relationships>`;

const rootRels = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
<Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>`;

const contentTypes = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
<Default Extension="xml" ContentType="application/xml"/>
<Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
<Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
<Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>
<Override PartName="/word/settings.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"/>
<Override PartName="/word/header1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"/>
<Override PartName="/word/footer1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>
<Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
<Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
</Types>`;

const settingsXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:settings xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:zoom w:percent="100"/><w:defaultTabStop w:val="720"/></w:settings>`;
const headerXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:hdr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">${para("Host Operations Database Brief", "Meta")}</w:hdr>`;
const footerXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:ftr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">${para("Database flow và đề xuất cải thiện - Host App", "Meta", { align: "center" })}</w:ftr>`;

const now = new Date().toISOString();
const coreXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:dcmitype="http://purl.org/dc/dcmitype/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><dc:title>Database Flow Host Operations</dc:title><dc:creator>Codex</dc:creator><cp:lastModifiedBy>Codex</cp:lastModifiedBy><dcterms:created xsi:type="dcterms:W3CDTF">${now}</dcterms:created><dcterms:modified xsi:type="dcterms:W3CDTF">${now}</dcterms:modified></cp:coreProperties>`;
const appXml = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"><Application>Codex</Application></Properties>`;

cleanDir(buildDir);
ensureDir(outDir);
ensureDir(path.join(buildDir, "_rels"));
ensureDir(path.join(buildDir, "word", "_rels"));
ensureDir(path.join(buildDir, "docProps"));

fs.writeFileSync(path.join(buildDir, "[Content_Types].xml"), contentTypes, "utf8");
fs.writeFileSync(path.join(buildDir, "_rels", ".rels"), rootRels, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "document.xml"), documentXml, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "_rels", "document.xml.rels"), documentRels, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "styles.xml"), stylesXml, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "numbering.xml"), numberingXml, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "settings.xml"), settingsXml, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "header1.xml"), headerXml, "utf8");
fs.writeFileSync(path.join(buildDir, "word", "footer1.xml"), footerXml, "utf8");
fs.writeFileSync(path.join(buildDir, "docProps", "core.xml"), coreXml, "utf8");
fs.writeFileSync(path.join(buildDir, "docProps", "app.xml"), appXml, "utf8");

fs.rmSync(outFile, { force: true });
const archive = outFile.replace(/\.docx$/i, ".zip");
fs.rmSync(archive, { force: true });
const ps = `Compress-Archive -Path '${buildDir.replace(/'/g, "''")}\\*' -DestinationPath '${archive.replace(/'/g, "''")}' -Force; Move-Item -LiteralPath '${archive.replace(/'/g, "''")}' -Destination '${outFile.replace(/'/g, "''")}' -Force`;
childProcess.execFileSync("powershell", ["-NoProfile", "-Command", ps], { stdio: "inherit" });

console.log(outFile);
