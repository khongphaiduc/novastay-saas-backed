using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetCategoryAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Assets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Assets");
        }
    }
}
