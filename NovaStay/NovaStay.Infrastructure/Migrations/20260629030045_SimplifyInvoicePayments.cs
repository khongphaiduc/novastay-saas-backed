using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyInvoicePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "PaymentReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM PaymentAllocations
                    GROUP BY PaymentReceiptId
                    HAVING COUNT(DISTINCT InvoiceId) > 1
                )
                BEGIN
                    THROW 51000, 'Cannot simplify invoice payments because at least one payment receipt is allocated to multiple invoices.', 1;
                END
                """);

            migrationBuilder.Sql("""
                UPDATE pr
                SET pr.InvoiceId = pa.InvoiceId
                FROM PaymentReceipts pr
                INNER JOIN (
                    SELECT PaymentReceiptId, MIN(InvoiceId) AS InvoiceId
                    FROM PaymentAllocations
                    GROUP BY PaymentReceiptId
                ) pa ON pa.PaymentReceiptId = pr.Id
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM PaymentReceipts
                    WHERE InvoiceId IS NULL
                )
                BEGIN
                    THROW 51001, 'Cannot simplify invoice payments because at least one payment receipt is not linked to an invoice.', 1;
                END
                """);

            migrationBuilder.Sql("""
                UPDATE i
                SET
                    i.Status = 'Paid',
                    i.PaidAt = COALESCE(pr.PaidAt, i.PaidAt)
                FROM Invoices i
                INNER JOIN PaymentReceipts pr ON pr.InvoiceId = i.Id
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                table: "PaymentReceipts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_PaymentReceipts_InvoiceId",
                table: "PaymentReceipts",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Invoices_InvoiceId",
                table: "PaymentReceipts",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OutstandingAmount",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_PaymentReceipts_PaymentReceiptId",
                        column: x => x.PaymentReceiptId,
                        principalTable: "PaymentReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_InvoiceId",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "UX_PaymentAllocations_PaymentReceiptId_InvoiceId",
                table: "PaymentAllocations",
                columns: new[] { "PaymentReceiptId", "InvoiceId" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO PaymentAllocations (Id, PaymentReceiptId, InvoiceId, AllocatedAmount, CreatedAt)
                SELECT
                    NEWSEQUENTIALID(),
                    pr.Id,
                    pr.InvoiceId,
                    pr.Amount,
                    pr.CreatedAt
                FROM PaymentReceipts pr
                """);

            migrationBuilder.Sql("""
                UPDATE i
                SET
                    i.AmountPaid = CASE WHEN pr.Id IS NOT NULL THEN i.TotalAmount ELSE 0 END,
                    i.OutstandingAmount = CASE WHEN pr.Id IS NOT NULL THEN 0 ELSE i.TotalAmount END
                FROM Invoices i
                LEFT JOIN PaymentReceipts pr ON pr.InvoiceId = i.Id
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Invoices_InvoiceId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "UX_PaymentReceipts_InvoiceId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PaymentReceipts");
        }
    }
}
