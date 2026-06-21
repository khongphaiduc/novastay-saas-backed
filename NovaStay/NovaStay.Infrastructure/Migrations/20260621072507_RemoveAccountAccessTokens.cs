using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAccessTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountAccessTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedByIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RevokedByIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    TokenHash = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountAccessTokens_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccessTokens_AccountId_ExpiresAt",
                table: "AccountAccessTokens",
                columns: new[] { "AccountId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccessTokens_TokenHash",
                table: "AccountAccessTokens",
                column: "TokenHash");
        }
    }
}
