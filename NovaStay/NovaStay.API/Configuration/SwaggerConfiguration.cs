using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NovaStay.API.Configuration;

// Swagger chỉ liệt kê các endpoint frontend đang gọi (đã đối chiếu với src/api và các fetch trực tiếp trong component).
// Endpoint chưa được frontend dùng (ví dụ xóa cư dân khỏi cơ sở, link mời đang tắt) không xuất hiện.
// Khi frontend thêm hoặc bỏ một lời gọi API, cập nhật danh sách UsedEndpoints bên dưới.
public static class SwaggerConfiguration
{
    private static readonly HashSet<string> UsedEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        // Xác thực
        "POST api/auth/register-organization",
        "POST api/auth/business/login",
        "POST api/auth/reset-password",
        "POST api/auth/resident/login",
        "POST api/auth/resident/register",
        "POST api/auth/resident/google-login",
        "POST api/auth/change-password",
        "POST api/auth/refresh",
        "POST api/auth/logout",

        // Quản trị hệ thống
        "GET api/admin/dashboard/stats",
        "GET api/admin/organizations",
        "GET api/admin/residents",

        // Tài sản
        "GET api/assets",
        "POST api/assets",
        "GET api/assets/paged",
        "GET api/assets/by-room",
        "GET api/assets/statistics",
        "PUT api/assets/{}",
        "DELETE api/assets/{}",
        "POST api/assets/{}/assign",
        "POST api/assets/{}/revoke",
        "PATCH api/assets/{}/status",
        "GET api/assets/{}/history",

        // Hóa đơn, thanh toán, tài khoản nhận tiền
        "GET api/billing/bank-account",
        "PUT api/billing/bank-account",
        "GET api/billing/bank-accounts/{}/qr",
        "GET api/billing/properties",
        "GET api/billing/preview",
        "GET api/billing/invoices",
        "POST api/billing/invoices",
        "GET api/billing/invoices/paged",
        "POST api/billing/invoices/{}/confirm",
        "GET api/billing/me/invoices",
        "POST api/billing/me/invoices/{}/report-transfer",
        "GET api/billing/payment-capabilities",
        "GET api/billing/invoices/{}/payment-status",
        "POST api/billing/invoices/{}/payment-link",
        "POST api/billing/invoices/{}/reconcile",
        // Webhook do PayOS gọi, không do frontend gọi
        "POST api/payments/payos/webhook",

        // Hợp đồng
        "GET api/contracts",
        "POST api/contracts",
        "PUT api/contracts/{}",
        "POST api/contracts/{}/renew",
        "POST api/contracts/{}/terminate",
        "POST api/contracts/{}/upload-pdf",
        "POST api/contracts/{}/send-renewal-notice",
        "GET api/contracts/expiring-soon",
        "GET api/contracts/summary",
        "GET api/contracts/{}/occupants",
        "GET api/contracts/{}/co-resident-candidates",
        "PUT api/contracts/{}/co-residents",
        "GET api/organizations/{}/room-occupancies",

        // Thu chi
        "GET api/organizations/{}/properties/{}/financial-transactions",
        "GET api/organizations/{}/properties/{}/expenses/paged",
        "POST api/organizations/{}/properties/{}/expenses",
        "GET api/organizations/{}/properties/{}/expenses/{}",
        "PATCH api/organizations/{}/properties/{}/expenses/{}/status",
        "GET api/organizations/{}/properties/{}/income-receipts/paged",
        "POST api/organizations/{}/properties/{}/income-receipts",
        "GET api/organizations/{}/properties/{}/income-receipts/{}",
        "PATCH api/organizations/{}/properties/{}/income-receipts/{}/status",

        // Cư dân của tổ chức và lời mời cư dân
        "GET api/organizations/{}/residents",
        "GET api/organizations/{}/residents/paged",
        "GET api/organizations/{}/residents/invitations",
        "POST api/organizations/{}/residents/invitations",
        "GET api/resident-invitations/pending",
        "POST api/resident-memberships/{}/accept",

        // Cơ sở và dịch vụ
        "GET api/organizations/{}/properties",
        "POST api/organizations/{}/properties",
        "PUT api/organizations/{}/properties/{}",
        "DELETE api/organizations/{}/properties/{}",
        "GET api/organizations/{}/properties/{}/services",
        "GET api/organizations/{}/properties/{}/services/paged",
        "POST api/organizations/{}/properties/{}/services",
        "PUT api/organizations/{}/properties/{}/services/{}",

        // Cư dân (hồ sơ, tra cứu, hồ sơ cá nhân)
        "POST api/residents",
        "GET api/residents/search",
        "PUT api/residents/{}",
        "POST api/residents/{}/images/{}",
        "GET api/residents/me",
        "PUT api/residents/me/profile",
        "POST api/residents/me/avatar",
        "GET api/residents/me/room",
        "GET api/residents/me/accommodations",
        "GET api/residents/me/contracts",
        "GET api/residents/me/invoices",
        "GET api/residents/me/incidents",
        "POST api/residents/me/incidents",

        // Phòng trọ
        "GET api/rooms",
        "POST api/rooms",
        "PUT api/rooms/{}",
        "DELETE api/rooms/{}",
        "GET api/rooms/summary",
        "GET api/rooms/proxy-image",
        "POST api/rooms/{}/images",
        "DELETE api/rooms/{}/images/{}",
        "PATCH api/rooms/{}/price",
        "PATCH api/rooms/{}/occupants",
        "PATCH api/rooms/{}/amenities",

        // Bảo trì
        "GET api/maintenance/rooms/{}",
        "PATCH api/maintenance/rooms/{}/status",
        "GET api/maintenance/tickets",
        "POST api/maintenance/tickets",
        "PATCH api/maintenance/tickets/{}/status",

        // Đồng hồ điện nước và chỉ số
        "GET api/utility-meters/by-property/{}",
        "GET api/utility-meters/by-property/{}/paged",
        "GET api/utility-meters/by-room/{}",
        "GET api/utility-meters/by-room/{}/paged",
        "POST api/utility-meters",
        "GET api/utility-meters/{}/readings",
        "GET api/utility-meters/{}/readings/paged",
        "POST api/utility-meters/{}/readings",
    };

    public static void AddNovaStaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NovaStay API",
                Version = "v1",
                Description = "Các endpoint đang được frontend NovaStay sử dụng."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Dán access token (không cần thêm chữ Bearer)."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            options.DocInclusionPredicate((_, description) =>
                description.HttpMethod is not null &&
                UsedEndpoints.Contains(Key(description.HttpMethod, description.RelativePath ?? string.Empty)));
        });
    }

    // Swagger bật cùng backend. Đặt Swagger:Enabled=false để tắt (ví dụ trên VPS nếu không muốn công bố tài liệu).
    public static void UseNovaStaySwagger(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Swagger:Enabled", true)) return;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "NovaStay API v1");
            options.RoutePrefix = "swagger";
        });
    }

    private static string Key(string method, string relativePath)
    {
        var template = Regex.Replace(relativePath.Trim('/'), @"\{[^}]*\}", "{}");
        return $"{method.ToUpperInvariant()} {template}";
    }
}
