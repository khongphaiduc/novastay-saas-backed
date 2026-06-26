using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveServicesToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomServices_OrganizationServices_OrganizationServiceId",
                table: "RoomServices");

            migrationBuilder.DropIndex(
                name: "UX_RoomServices_RoomId_OrganizationServiceId",
                table: "RoomServices");

            migrationBuilder.DropIndex(
                name: "IX_RoomServices_OrganizationServiceId",
                table: "RoomServices");

            migrationBuilder.CreateTable(
                name: "PropertyServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ServiceCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BillingCycle = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    OldOrganizationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyServices_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(
                """
                INSERT INTO PropertyServices (
                    Id,
                    PropertyId,
                    ServiceName,
                    ServiceCode,
                    Description,
                    DefaultPrice,
                    Unit,
                    BillingCycle,
                    IsActive,
                    CreatedAt,
                    UpdatedAt,
                    OldOrganizationServiceId)
                SELECT
                    NEWID(),
                    p.Id,
                    os.ServiceName,
                    os.ServiceCode,
                    os.Description,
                    os.DefaultPrice,
                    os.Unit,
                    os.BillingCycle,
                    os.IsActive,
                    os.CreatedAt,
                    os.UpdatedAt,
                    os.Id
                FROM OrganizationServices os
                INNER JOIN Properties p ON p.OrganizationId = os.OrganizationId;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyServiceId",
                table: "RoomServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE rs
                SET PropertyServiceId = ps.Id
                FROM RoomServices rs
                INNER JOIN Rooms r ON r.Id = rs.RoomId
                INNER JOIN PropertyServices ps
                    ON ps.PropertyId = r.PropertyId
                    AND ps.OldOrganizationServiceId = rs.OrganizationServiceId;

                DELETE FROM RoomServices
                WHERE PropertyServiceId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "OrganizationServiceId",
                table: "RoomServices");

            migrationBuilder.DropTable(
                name: "OrganizationServices");

            migrationBuilder.DropColumn(
                name: "OldOrganizationServiceId",
                table: "PropertyServices");

            migrationBuilder.AlterColumn<Guid>(
                name: "PropertyServiceId",
                table: "RoomServices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyServices_PropertyId_IsActive",
                table: "PropertyServices",
                columns: new[] { "PropertyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_PropertyServices_PropertyId_ServiceName",
                table: "PropertyServices",
                columns: new[] { "PropertyId", "ServiceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomServices_PropertyServiceId",
                table: "RoomServices",
                column: "PropertyServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_RoomServices_RoomId_PropertyServiceId",
                table: "RoomServices",
                columns: new[] { "RoomId", "PropertyServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomServices_PropertyServices_PropertyServiceId",
                table: "RoomServices",
                column: "PropertyServiceId",
                principalTable: "PropertyServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomServices_PropertyServices_PropertyServiceId",
                table: "RoomServices");

            migrationBuilder.DropIndex(
                name: "IX_RoomServices_PropertyServiceId",
                table: "RoomServices");

            migrationBuilder.DropIndex(
                name: "UX_RoomServices_RoomId_PropertyServiceId",
                table: "RoomServices");

            migrationBuilder.CreateTable(
                name: "OrganizationServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingCycle = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ServiceCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    OldPropertyServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationServices_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(
                """
                WITH RankedPropertyServices AS (
                    SELECT
                        ps.*,
                        p.OrganizationId,
                        ROW_NUMBER() OVER (
                            PARTITION BY p.OrganizationId, ps.ServiceName
                            ORDER BY ps.CreatedAt, ps.Id) AS RowNumber
                    FROM PropertyServices ps
                    INNER JOIN Properties p ON p.Id = ps.PropertyId
                )
                INSERT INTO OrganizationServices (
                    Id,
                    OrganizationId,
                    BillingCycle,
                    CreatedAt,
                    DefaultPrice,
                    Description,
                    IsActive,
                    ServiceCode,
                    ServiceName,
                    Unit,
                    UpdatedAt,
                    OldPropertyServiceId)
                SELECT
                    NEWID(),
                    OrganizationId,
                    BillingCycle,
                    CreatedAt,
                    DefaultPrice,
                    Description,
                    IsActive,
                    ServiceCode,
                    ServiceName,
                    Unit,
                    UpdatedAt,
                    Id
                FROM RankedPropertyServices
                WHERE RowNumber = 1;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationServiceId",
                table: "RoomServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE rs
                SET OrganizationServiceId = os.Id
                FROM RoomServices rs
                INNER JOIN PropertyServices ps ON ps.Id = rs.PropertyServiceId
                INNER JOIN Properties p ON p.Id = ps.PropertyId
                INNER JOIN OrganizationServices os
                    ON os.OrganizationId = p.OrganizationId
                    AND os.ServiceName = ps.ServiceName;

                DELETE FROM RoomServices
                WHERE OrganizationServiceId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PropertyServiceId",
                table: "RoomServices");

            migrationBuilder.DropTable(
                name: "PropertyServices");

            migrationBuilder.DropColumn(
                name: "OldPropertyServiceId",
                table: "OrganizationServices");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationServiceId",
                table: "RoomServices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationServices_OrganizationId_IsActive",
                table: "OrganizationServices",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationServices_OrganizationId_ServiceName",
                table: "OrganizationServices",
                columns: new[] { "OrganizationId", "ServiceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomServices_OrganizationServiceId",
                table: "RoomServices",
                column: "OrganizationServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_RoomServices_RoomId_OrganizationServiceId",
                table: "RoomServices",
                columns: new[] { "RoomId", "OrganizationServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomServices_OrganizationServices_OrganizationServiceId",
                table: "RoomServices",
                column: "OrganizationServiceId",
                principalTable: "OrganizationServices",
                principalColumn: "Id");
        }
    }
}
