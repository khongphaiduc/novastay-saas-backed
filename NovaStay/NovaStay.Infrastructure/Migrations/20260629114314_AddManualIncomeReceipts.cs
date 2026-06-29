using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualIncomeReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncomeCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeCategories_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomeReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncomeCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    IncomeType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ReferenceCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_IncomeCategories_IncomeCategoryId",
                        column: x => x.IncomeCategoryId,
                        principalTable: "IncomeCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeReceipts_StaffUsers_CollectedByStaffUserId",
                        column: x => x.CollectedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "UX_IncomeCategories_OrganizationId_CategoryCode",
                table: "IncomeCategories",
                columns: new[] { "OrganizationId", "CategoryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_CollectedByStaffUserId",
                table: "IncomeReceipts",
                column: "CollectedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_IncomeCategoryId",
                table: "IncomeReceipts",
                column: "IncomeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_OrganizationId_CollectedAt",
                table: "IncomeReceipts",
                columns: new[] { "OrganizationId", "CollectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_PropertyId_IncomeCategoryId_CollectedAt",
                table: "IncomeReceipts",
                columns: new[] { "PropertyId", "IncomeCategoryId", "CollectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_ResidentId",
                table: "IncomeReceipts",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeReceipts_RoomId",
                table: "IncomeReceipts",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_IncomeReceipts_OrganizationId_ReceiptNumber",
                table: "IncomeReceipts",
                columns: new[] { "OrganizationId", "ReceiptNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomeReceipts");

            migrationBuilder.DropTable(
                name: "IncomeCategories");
        }
    }
}
