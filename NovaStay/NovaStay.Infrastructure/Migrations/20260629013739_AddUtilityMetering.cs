using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityMetering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UtilityMeters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeterCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MeterType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InitialReading = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityMeters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilityMeters_PropertyServices_PropertyServiceId",
                        column: x => x.PropertyServiceId,
                        principalTable: "PropertyServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UtilityMeters_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UtilityTariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    PropertyServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricingMode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityTariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilityTariffs_PropertyServices_PropertyServiceId",
                        column: x => x.PropertyServiceId,
                        principalTable: "PropertyServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UtilityReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    UtilityMeterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadingDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    BillingPeriod = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    PreviousReading = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CurrentReading = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Consumption = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Draft"),
                    RecordedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilityReadings_StaffUsers_RecordedByStaffUserId",
                        column: x => x.RecordedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UtilityReadings_UtilityMeters_UtilityMeterId",
                        column: x => x.UtilityMeterId,
                        principalTable: "UtilityMeters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMeters_PropertyServiceId",
                table: "UtilityMeters",
                column: "PropertyServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMeters_RoomId",
                table: "UtilityMeters",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_UtilityMeters_MeterCode",
                table: "UtilityMeters",
                column: "MeterCode",
                unique: true,
                filter: "[MeterCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_UtilityMeters_RoomId_PropertyServiceId",
                table: "UtilityMeters",
                columns: new[] { "RoomId", "PropertyServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_RecordedByStaffUserId",
                table: "UtilityReadings",
                column: "RecordedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_Status_BillingPeriod",
                table: "UtilityReadings",
                columns: new[] { "Status", "BillingPeriod" });

            migrationBuilder.CreateIndex(
                name: "UX_UtilityReadings_UtilityMeterId_BillingPeriod",
                table: "UtilityReadings",
                columns: new[] { "UtilityMeterId", "BillingPeriod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_UtilityTariffs_PropertyServiceId_EffectiveFrom",
                table: "UtilityTariffs",
                columns: new[] { "PropertyServiceId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UtilityReadings");

            migrationBuilder.DropTable(
                name: "UtilityTariffs");

            migrationBuilder.DropTable(
                name: "UtilityMeters");
        }
    }
}
