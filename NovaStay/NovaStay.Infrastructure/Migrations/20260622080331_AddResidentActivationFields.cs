using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentActivationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "ResidentMemberships",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MembershipCode",
                table: "ResidentMemberships",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ResidentMemberships
                SET MembershipCode = 'RM-' + LEFT(REPLACE(CONVERT(varchar(36), Id), '-', ''), 12)
                WHERE MembershipCode IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "MembershipCode",
                table: "ResidentMemberships",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Accounts",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AddColumn<bool>(
                name: "MustSetPassword",
                table: "Accounts",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordSetAt",
                table: "Accounts",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_ResidentMemberships_MembershipCode",
                table: "ResidentMemberships",
                column: "MembershipCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ResidentMemberships_MembershipCode",
                table: "ResidentMemberships");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "ResidentMemberships");

            migrationBuilder.DropColumn(
                name: "MembershipCode",
                table: "ResidentMemberships");

            migrationBuilder.DropColumn(
                name: "MustSetPassword",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordSetAt",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Accounts",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
