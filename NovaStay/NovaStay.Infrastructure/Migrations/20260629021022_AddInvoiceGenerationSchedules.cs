using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceGenerationSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceGenerationSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BillingCycle = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    GenerateDayOfMonth = table.Column<int>(type: "int", nullable: false),
                    GenerateTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DueAfterDays = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    AutoSendToResident = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceGenerationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceGenerationSchedules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceGenerationSchedules_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceGenerationSchedules_StaffUsers_CreatedByStaffUserId",
                        column: x => x.CreatedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceGenerationSchedules_CreatedByStaffUserId",
                table: "InvoiceGenerationSchedules",
                column: "CreatedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceGenerationSchedules_IsActive_NextRunAt",
                table: "InvoiceGenerationSchedules",
                columns: new[] { "IsActive", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceGenerationSchedules_OrganizationId",
                table: "InvoiceGenerationSchedules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceGenerationSchedules_PropertyId",
                table: "InvoiceGenerationSchedules",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "UX_InvoiceGenerationSchedules_OrganizationId_ScheduleName",
                table: "InvoiceGenerationSchedules",
                columns: new[] { "OrganizationId", "ScheduleName" },
                unique: true,
                filter: "[PropertyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_InvoiceGenerationSchedules_OrganizationId_PropertyId_ScheduleName",
                table: "InvoiceGenerationSchedules",
                columns: new[] { "OrganizationId", "PropertyId", "ScheduleName" },
                unique: true,
                filter: "[PropertyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceGenerationSchedules");
        }
    }
}
