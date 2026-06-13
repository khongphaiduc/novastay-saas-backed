using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Host.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__AssetAssi__BedId__0A9D95DB",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK__Bookings__BedId__1EA48E88",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK__Contracts__BedId__2FCF1A8A",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK__Listings__BedId__123EB7A3",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropIndex(
                name: "IX_Listings_BedId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_BedId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BedId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_BedId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_RoomId_BedId",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "BedId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BedId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "BedId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BedId",
                table: "AssetAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_RoomId",
                table: "AssetAssignments",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_RoomId",
                table: "AssetAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                table: "Listings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                table: "AssetAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BedNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CardToken = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    LockerId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Beds__3214EC07D3D03E54", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Beds__RoomId__7A672E12",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_BedId",
                table: "Listings",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BedId",
                table: "Contracts",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BedId",
                table: "Bookings",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_BedId",
                table: "AssetAssignments",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_RoomId_BedId",
                table: "AssetAssignments",
                columns: new[] { "RoomId", "BedId" });

            migrationBuilder.CreateIndex(
                name: "IX_Beds_RoomId",
                table: "Beds",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK__AssetAssi__BedId__0A9D95DB",
                table: "AssetAssignments",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Bookings__BedId__1EA48E88",
                table: "Bookings",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Contracts__BedId__2FCF1A8A",
                table: "Contracts",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Listings__BedId__123EB7A3",
                table: "Listings",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id");
        }
    }
}
