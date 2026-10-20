using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvitationLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Token = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    MaxUses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UsedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationLinks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvitationLinks_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvitationLinks_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvitationLinks_OrganizationId_IsActive",
                table: "InvitationLinks",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InvitationLinks_PropertyId",
                table: "InvitationLinks",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationLinks_RoomId",
                table: "InvitationLinks",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_InvitationLinks_Token",
                table: "InvitationLinks",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvitationLinks");
        }
    }
}
