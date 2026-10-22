using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualTransferBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity", table: "InvoiceLines", type: "decimal(18, 3)", nullable: false,
                defaultValue: 1m, oldClrType: typeof(decimal), oldType: "decimal(18, 2)", oldDefaultValue: 1m);
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Phone",
                table: "Accounts");

            migrationBuilder.AddColumn<Guid>(
                name: "BillingBankAccountId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BillingVersion",
                table: "Invoices",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationNote",
                table: "Invoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByAccountId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferReportedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingOrganizationId",
                table: "InvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPeriod",
                table: "InvoiceLines",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingRoomId",
                table: "InvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BillingBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    QrImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    QrContentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingBankAccounts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BillingBankAccountId",
                table: "Invoices",
                column: "BillingBankAccountId");

            migrationBuilder.CreateIndex(
                name: "UX_InvoiceLines_BillingCharge",
                table: "InvoiceLines",
                columns: new[] { "BillingOrganizationId", "BillingRoomId", "BillingPeriod", "SourceType", "SourceId" },
                unique: true,
                filter: "[BillingPeriod] IS NOT NULL AND [SourceId] IS NOT NULL AND [SourceType] IS NOT NULL AND [BillingOrganizationId] IS NOT NULL AND [BillingRoomId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Phone",
                table: "Accounts",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL AND [Phone] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_BillingBankAccounts_OrganizationId_CreatedAt",
                table: "BillingBankAccounts",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_BillingBankAccounts_BillingBankAccountId",
                table: "Invoices",
                column: "BillingBankAccountId",
                principalTable: "BillingBankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity", table: "InvoiceLines", type: "decimal(18, 2)", nullable: false,
                defaultValue: 1m, oldClrType: typeof(decimal), oldType: "decimal(18, 3)", oldDefaultValue: 1m);
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_BillingBankAccounts_BillingBankAccountId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "BillingBankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BillingBankAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "UX_InvoiceLines_BillingCharge",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Phone",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BillingBankAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BillingVersion",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ConfirmationNote",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ConfirmedByAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TransferReportedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BillingOrganizationId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "BillingPeriod",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "BillingRoomId",
                table: "InvoiceLines");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Phone",
                table: "Accounts",
                column: "Phone",
                unique: true);
        }
    }
}
