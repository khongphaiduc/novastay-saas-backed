using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentMembershipInvitationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ResidentMemberships",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitedAt",
                table: "ResidentMemberships",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "ResidentMemberships",
                type: "datetime",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ResidentMemberships
                SET InvitedAt = COALESCE(CreatedAt, GETDATE())
                WHERE InvitedAt IS NULL;

                UPDATE ResidentMemberships
                SET RespondedAt = COALESCE(ActivatedAt, CreatedAt, GETDATE())
                WHERE Status = 'Active'
                  AND RespondedAt IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitedAt",
                table: "ResidentMemberships");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "ResidentMemberships");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ResidentMemberships",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldDefaultValue: "Pending");
        }
    }
}
