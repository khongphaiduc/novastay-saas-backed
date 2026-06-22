using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResidentMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Active"),
                    JoinedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentMemberships_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResidentMemberships_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResidentMemberships_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMemberships_AccountId",
                table: "ResidentMemberships",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMemberships_OrganizationId",
                table: "ResidentMemberships",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMemberships_ResidentId",
                table: "ResidentMemberships",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "UX_ResidentMemberships_AccountId_OrganizationId",
                table: "ResidentMemberships",
                columns: new[] { "AccountId", "OrganizationId" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO ResidentMemberships (AccountId, ResidentId, OrganizationId, Status, JoinedAt, CreatedAt)
                SELECT AccountId, Id, OrganizationId, 'Active', CreatedAt, CreatedAt
                FROM Residents
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ResidentMemberships
                    WHERE ResidentMemberships.AccountId = Residents.AccountId
                      AND ResidentMemberships.OrganizationId = Residents.OrganizationId
                )
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK__Residents__Tenan__22751F6C'
                      AND parent_object_id = OBJECT_ID('Residents')
                )
                BEGIN
                    ALTER TABLE Residents DROP CONSTRAINT FK__Residents__Tenan__22751F6C;
                END
                """);

            migrationBuilder.Sql("""
                DECLARE @dropResidentOrganizationIndexes nvarchar(max) = N'';

                SELECT @dropResidentOrganizationIndexes = @dropResidentOrganizationIndexes
                    + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON Residents;'
                FROM sys.indexes AS i
                INNER JOIN sys.index_columns AS ic
                    ON i.object_id = ic.object_id
                   AND i.index_id = ic.index_id
                INNER JOIN sys.columns AS c
                    ON ic.object_id = c.object_id
                   AND ic.column_id = c.column_id
                WHERE i.object_id = OBJECT_ID('Residents')
                  AND c.name = 'OrganizationId'
                  AND i.is_primary_key = 0
                  AND i.name IS NOT NULL;

                IF @dropResidentOrganizationIndexes <> N''
                BEGIN
                    EXEC sp_executesql @dropResidentOrganizationIndexes;
                END
                """);

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Residents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Residents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE Residents
                SET OrganizationId = ResidentMemberships.OrganizationId
                FROM Residents
                INNER JOIN ResidentMemberships ON ResidentMemberships.ResidentId = Residents.Id
                WHERE ResidentMemberships.Id = (
                    SELECT TOP 1 Id
                    FROM ResidentMemberships AS SelectedMembership
                    WHERE SelectedMembership.ResidentId = Residents.Id
                    ORDER BY SelectedMembership.CreatedAt
                )
                """);

            migrationBuilder.DropTable(
                name: "ResidentMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_OrganizationId",
                table: "Residents",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK__Residents__Tenan__22751F6C",
                table: "Residents",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
