using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260627_AddFinancialManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_OrganizationId",
                table: "Invoices");

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustmentAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "Invoices",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Invoices",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceType",
                table: "Invoices",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "Invoices",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LateFeeAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Invoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResidentId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoices",
                type: "datetime",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE i
                SET
                    PropertyId = COALESCE(c.PropertyId, b.PropertyId),
                    RoomId = COALESCE(c.RoomId, b.RoomId),
                    ResidentId = c.ResidentId,
                    InvoiceNumber = CONCAT(
                        'INV-',
                        REPLACE(ISNULL(i.InvoicePeriod, CONVERT(varchar(7), GETDATE(), 120)), '-', ''),
                        '-',
                        RIGHT(CONVERT(varchar(36), i.Id), 8)),
                    InvoiceType = CASE
                        WHEN i.ContractId IS NOT NULL THEN 'Contract'
                        WHEN i.BookingId IS NOT NULL THEN 'Booking'
                        ELSE 'General'
                    END,
                    IssuedAt = COALESCE(i.CreatedAt, GETDATE()),
                    DueDate = COALESCE(
                        EOMONTH(TRY_CONVERT(date, i.InvoicePeriod + '-01')),
                        CAST(COALESCE(i.CreatedAt, GETDATE()) AS date)),
                    Subtotal = ISNULL(i.RoomPrice, 0) + ISNULL(i.ServicesPrice, 0),
                    OutstandingAmount = CASE
                        WHEN i.PaidAt IS NOT NULL OR i.Status = 'Paid' THEN 0
                        ELSE ISNULL(i.TotalAmount, 0)
                    END,
                    AmountPaid = CASE
                        WHEN i.PaidAt IS NOT NULL OR i.Status = 'Paid' THEN ISNULL(i.TotalAmount, 0)
                        ELSE 0
                    END,
                    Status = CASE
                        WHEN i.PaidAt IS NOT NULL OR i.Status = 'Paid' THEN 'Paid'
                        WHEN i.Status IS NULL OR i.Status = 'Unpaid' THEN 'Issued'
                        ELSE i.Status
                    END,
                    UpdatedAt = COALESCE(i.UpdatedAt, i.CreatedAt, GETDATE())
                FROM Invoices i
                LEFT JOIN Contracts c ON c.Id = i.ContractId
                LEFT JOIN Bookings b ON b.Id = i.BookingId;
                """);

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
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
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(
                """
                INSERT INTO ExpenseCategories (Id, OrganizationId, CategoryCode, CategoryName, IsActive, CreatedAt)
                SELECT NEWID(), o.Id, seed.CategoryCode, seed.CategoryName, 1, GETDATE()
                FROM Organizations o
                CROSS JOIN (
                    VALUES
                        ('OPERATING', N'Operating Expense'),
                        ('MAINTENANCE', N'Maintenance Expense'),
                        ('COMMISSION', N'Broker Commission'),
                        ('REFUND', N'Refund'),
                        ('OTHER', N'Other Expense')
                ) AS seed(CategoryCode, CategoryName)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ExpenseCategories ec
                    WHERE ec.OrganizationId = o.Id
                      AND ec.CategoryCode = seed.CategoryCode
                );
                """);

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    SourceType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 1m),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BillingEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDebit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(
                """
                INSERT INTO InvoiceLines (
                    Id,
                    InvoiceId,
                    LineType,
                    Description,
                    Quantity,
                    UnitPrice,
                    LineAmount,
                    DisplayOrder,
                    IsDebit,
                    CreatedAt)
                SELECT
                    NEWID(),
                    i.Id,
                    'RoomRent',
                    N'Room charge',
                    1,
                    i.RoomPrice,
                    i.RoomPrice,
                    1,
                    1,
                    COALESCE(i.CreatedAt, GETDATE())
                FROM Invoices i
                WHERE ISNULL(i.RoomPrice, 0) <> 0;

                INSERT INTO InvoiceLines (
                    Id,
                    InvoiceId,
                    LineType,
                    Description,
                    Quantity,
                    UnitPrice,
                    LineAmount,
                    DisplayOrder,
                    IsDebit,
                    CreatedAt)
                SELECT
                    NEWID(),
                    i.Id,
                    'Service',
                    N'Service charge',
                    1,
                    i.ServicesPrice,
                    i.ServicesPrice,
                    2,
                    1,
                    COALESCE(i.CreatedAt, GETDATE())
                FROM Invoices i
                WHERE ISNULL(i.ServicesPrice, 0) <> 0;
                """);

            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ReceiptType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReferenceCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_StaffUsers_CollectedByStaffUserId",
                        column: x => x.CollectedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedMaintenanceTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedBrokerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByStaffUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpenseNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ExpenseType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PayeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpentAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ReferenceCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Brokers_RelatedBrokerId",
                        column: x => x.RelatedBrokerId,
                        principalTable: "Brokers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_MaintenanceTickets_RelatedMaintenanceTicketId",
                        column: x => x.RelatedMaintenanceTicketId,
                        principalTable: "MaintenanceTickets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_StaffUsers_ApprovedByStaffUserId",
                        column: x => x.ApprovedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_StaffUsers_CreatedByStaffUserId",
                        column: x => x.CreatedByStaffUserId,
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    PaymentReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "IX_Invoices_PropertyId_RoomId_Period",
                table: "Invoices",
                columns: new[] { "PropertyId", "RoomId", "InvoicePeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ResidentId_Status",
                table: "Invoices",
                columns: new[] { "ResidentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RoomId",
                table: "Invoices",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_Invoices_OrganizationId_InvoiceNumber",
                table: "Invoices",
                columns: new[] { "OrganizationId", "InvoiceNumber" },
                unique: true,
                filter: "[InvoiceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_ExpenseCategories_OrganizationId_CategoryCode",
                table: "ExpenseCategories",
                columns: new[] { "OrganizationId", "CategoryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApprovedByStaffUserId",
                table: "Expenses",
                column: "ApprovedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedByStaffUserId",
                table: "Expenses",
                column: "CreatedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_OrganizationId_SpentAt",
                table: "Expenses",
                columns: new[] { "OrganizationId", "SpentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PropertyId_ExpenseCategoryId_SpentAt",
                table: "Expenses",
                columns: new[] { "PropertyId", "ExpenseCategoryId", "SpentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RelatedBrokerId",
                table: "Expenses",
                column: "RelatedBrokerId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RelatedMaintenanceTicketId",
                table: "Expenses",
                column: "RelatedMaintenanceTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RoomId",
                table: "Expenses",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_Expenses_OrganizationId_ExpenseNumber",
                table: "Expenses",
                columns: new[] { "OrganizationId", "ExpenseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId_DisplayOrder",
                table: "InvoiceLines",
                columns: new[] { "InvoiceId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_InvoiceId",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "UX_PaymentAllocations_PaymentReceiptId_InvoiceId",
                table: "PaymentAllocations",
                columns: new[] { "PaymentReceiptId", "InvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_CollectedByStaffUserId",
                table: "PaymentReceipts",
                column: "CollectedByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_OrganizationId_PaidAt",
                table: "PaymentReceipts",
                columns: new[] { "OrganizationId", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_PropertyId",
                table: "PaymentReceipts",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ResidentId",
                table: "PaymentReceipts",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "UX_PaymentReceipts_OrganizationId_ReceiptNumber",
                table: "PaymentReceipts",
                columns: new[] { "OrganizationId", "ReceiptNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Properties_PropertyId",
                table: "Invoices",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Residents_ResidentId",
                table: "Invoices",
                column: "ResidentId",
                principalTable: "Residents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Rooms_RoomId",
                table: "Invoices",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Properties_PropertyId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Residents_ResidentId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Rooms_RoomId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PropertyId_RoomId_Period",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ResidentId_Status",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_RoomId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "UX_Invoices_OrganizationId_InvoiceNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AdjustmentAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LateFeeAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OutstandingAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ResidentId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrganizationId",
                table: "Invoices",
                column: "OrganizationId");
        }
    }
}
