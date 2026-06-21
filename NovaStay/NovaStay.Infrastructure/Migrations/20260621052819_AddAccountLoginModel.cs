using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLoginModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Tenants]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[Organizations]', N'U') IS NULL
                    EXEC sp_rename N'[Tenants]', N'Organizations';

                IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[StaffUsers]', N'U') IS NULL
                    EXEC sp_rename N'[Users]', N'StaffUsers';

                IF OBJECT_ID(N'[UserAccessTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NULL
                    EXEC sp_rename N'[UserAccessTokens]', N'AccountAccessTokens';

                IF OBJECT_ID(N'[UserRefreshTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NULL
                    EXEC sp_rename N'[UserRefreshTokens]', N'AccountRefreshTokens';

                IF OBJECT_ID(N'[UserPropertyMapping]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[StaffUserPropertyMapping]', N'U') IS NULL
                    EXEC sp_rename N'[UserPropertyMapping]', N'StaffUserPropertyMapping';

                IF OBJECT_ID(N'[UserRoles]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[StaffUserRoles]', N'U') IS NULL
                    EXEC sp_rename N'[UserRoles]', N'StaffUserRoles';

                DECLARE @tableName sysname;
                DECLARE organization_column_cursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [name]
                    FROM sys.tables
                    WHERE [name] IN (
                        N'Assets',
                        N'AssetAssignments',
                        N'Bookings',
                        N'Brokers',
                        N'Contracts',
                        N'Invoices',
                        N'MaintenanceTickets',
                        N'Properties',
                        N'Residents',
                        N'Roles',
                        N'StaffUsers',
                        N'Technicians'
                    );

                OPEN organization_column_cursor;
                FETCH NEXT FROM organization_column_cursor INTO @tableName;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF COL_LENGTH(@tableName, N'TenantId') IS NOT NULL
                       AND COL_LENGTH(@tableName, N'OrganizationId') IS NULL
                    BEGIN
                        DECLARE @tenantColumnName nvarchar(300) = QUOTENAME(@tableName) + N'.[TenantId]';
                        EXEC sp_rename @tenantColumnName, N'OrganizationId', N'COLUMN';
                    END;

                    FETCH NEXT FROM organization_column_cursor INTO @tableName;
                END;
                CLOSE organization_column_cursor;
                DEALLOCATE organization_column_cursor;

                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'AccountAccessTokens', N'UserId') IS NOT NULL
                   AND COL_LENGTH(N'AccountAccessTokens', N'AccountId') IS NULL
                    EXEC sp_rename N'[AccountAccessTokens].[UserId]', N'AccountId', N'COLUMN';

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'AccountRefreshTokens', N'UserId') IS NOT NULL
                   AND COL_LENGTH(N'AccountRefreshTokens', N'AccountId') IS NULL
                    EXEC sp_rename N'[AccountRefreshTokens].[UserId]', N'AccountId', N'COLUMN';

                IF OBJECT_ID(N'[StaffUserPropertyMapping]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'StaffUserPropertyMapping', N'UserId') IS NOT NULL
                   AND COL_LENGTH(N'StaffUserPropertyMapping', N'StaffUserId') IS NULL
                    EXEC sp_rename N'[StaffUserPropertyMapping].[UserId]', N'StaffUserId', N'COLUMN';

                IF OBJECT_ID(N'[StaffUserRoles]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'StaffUserRoles', N'UserId') IS NOT NULL
                   AND COL_LENGTH(N'StaffUserRoles', N'StaffUserId') IS NULL
                    EXEC sp_rename N'[StaffUserRoles].[UserId]', N'StaffUserId', N'COLUMN';
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[FK_AccountAccessTokens_Users_UserId]', N'F') IS NOT NULL
                    ALTER TABLE [AccountAccessTokens] DROP CONSTRAINT [FK_AccountAccessTokens_Users_UserId];

                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[FK_UserAccessTokens_Users_UserId]', N'F') IS NOT NULL
                    ALTER TABLE [AccountAccessTokens] DROP CONSTRAINT [FK_UserAccessTokens_Users_UserId];

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[FK_AccountRefreshTokens_Users_UserId]', N'F') IS NOT NULL
                    ALTER TABLE [AccountRefreshTokens] DROP CONSTRAINT [FK_AccountRefreshTokens_Users_UserId];

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[FK_UserRefreshTokens_Users_UserId]', N'F') IS NOT NULL
                    ALTER TABLE [AccountRefreshTokens] DROP CONSTRAINT [FK_UserRefreshTokens_Users_UserId];

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'AccountRefreshTokens', N'StaffUserId') IS NOT NULL
                    EXEC sp_rename N'[AccountRefreshTokens].[StaffUserId]', N'AccountId', N'COLUMN';

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NOT NULL
                   AND EXISTS (
                       SELECT 1 FROM sys.indexes
                       WHERE name = N'IX_AccountRefreshTokens_UserId_ExpiresAt'
                         AND object_id = OBJECT_ID(N'[AccountRefreshTokens]')
                   )
                    EXEC sp_rename N'[AccountRefreshTokens].[IX_AccountRefreshTokens_UserId_ExpiresAt]', N'IX_AccountRefreshTokens_AccountId_ExpiresAt', N'INDEX';

                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'AccountAccessTokens', N'StaffUserId') IS NOT NULL
                    EXEC sp_rename N'[AccountAccessTokens].[StaffUserId]', N'AccountId', N'COLUMN';

                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NOT NULL
                   AND EXISTS (
                       SELECT 1 FROM sys.indexes
                       WHERE name = N'IX_AccountAccessTokens_UserId_ExpiresAt'
                         AND object_id = OBJECT_ID(N'[AccountAccessTokens]')
                   )
                    EXEC sp_rename N'[AccountAccessTokens].[IX_AccountAccessTokens_UserId_ExpiresAt]', N'IX_AccountAccessTokens_AccountId_ExpiresAt', N'INDEX';
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "StaffUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Residents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerAccountId",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    AccountType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO [Accounts] ([Id], [AccountType], [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt])
                SELECT [Id], 'Staff', [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt]
                FROM [StaffUsers]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Accounts] WHERE [Accounts].[Phone] = [StaffUsers].[Phone]
                );

                UPDATE [StaffUsers]
                SET [AccountId] = (
                    SELECT TOP 1 [Accounts].[Id]
                    FROM [Accounts]
                    WHERE [Accounts].[Phone] = [StaffUsers].[Phone]
                    ORDER BY [Accounts].[CreatedAt]
                );

                INSERT INTO [Accounts] ([Id], [AccountType], [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt])
                SELECT [Id], 'Resident', [Email], [Phone], 'resident-password-hash-change-me', 1, [CreatedAt]
                FROM [Residents]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Accounts] WHERE [Accounts].[Phone] = [Residents].[Phone]
                );

                UPDATE [Residents]
                SET [AccountId] = (
                    SELECT TOP 1 [Accounts].[Id]
                    FROM [Accounts]
                    WHERE [Accounts].[Phone] = [Residents].[Phone]
                    ORDER BY [Accounts].[CreatedAt]
                );

                INSERT INTO [Accounts] ([Id], [AccountType], [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt])
                SELECT [Id], 'BusinessOwner', [OwnerEmail], [OwnerPhone], 'owner-password-hash-change-me', 1, [CreatedAt]
                FROM [Organizations]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Accounts] WHERE [Accounts].[Phone] = [Organizations].[OwnerPhone]
                );

                UPDATE [Organizations]
                SET [OwnerAccountId] = (
                    SELECT TOP 1 [Accounts].[Id]
                    FROM [Accounts]
                    WHERE [Accounts].[Phone] = [Organizations].[OwnerPhone]
                    ORDER BY [Accounts].[CreatedAt]
                );
                """);

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "StaffUsers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "StaffUsers");

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[AccountAccessTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AccountAccessTokens] (
                        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
                        [AccountId] uniqueidentifier NOT NULL,
                        [TokenHash] varchar(512) NOT NULL,
                        [ExpiresAt] datetime NOT NULL,
                        [RevokedAt] datetime NULL,
                        [CreatedAt] datetime NULL DEFAULT ((getdate())),
                        [CreatedByIp] varchar(45) NULL,
                        [RevokedByIp] varchar(45) NULL,
                        CONSTRAINT [PK_AccountAccessTokens] PRIMARY KEY ([Id])
                    );
                END;

                IF OBJECT_ID(N'[AccountRefreshTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AccountRefreshTokens] (
                        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
                        [AccountId] uniqueidentifier NOT NULL,
                        [TokenHash] varchar(512) NOT NULL,
                        [ExpiresAt] datetime NOT NULL,
                        [RevokedAt] datetime NULL,
                        [CreatedAt] datetime NULL DEFAULT ((getdate())),
                        [CreatedByIp] varchar(45) NULL,
                        [RevokedByIp] varchar(45) NULL,
                        [ReplacedByTokenHash] varchar(512) NULL,
                        CONSTRAINT [PK_AccountRefreshTokens] PRIMARY KEY ([Id])
                    );
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_AccountAccessTokens_AccountId_ExpiresAt'
                      AND object_id = OBJECT_ID(N'[AccountAccessTokens]')
                )
                    CREATE INDEX [IX_AccountAccessTokens_AccountId_ExpiresAt]
                    ON [AccountAccessTokens] ([AccountId], [ExpiresAt]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_AccountAccessTokens_TokenHash'
                      AND object_id = OBJECT_ID(N'[AccountAccessTokens]')
                )
                    CREATE INDEX [IX_AccountAccessTokens_TokenHash]
                    ON [AccountAccessTokens] ([TokenHash]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_AccountRefreshTokens_AccountId_ExpiresAt'
                      AND object_id = OBJECT_ID(N'[AccountRefreshTokens]')
                )
                    CREATE INDEX [IX_AccountRefreshTokens_AccountId_ExpiresAt]
                    ON [AccountRefreshTokens] ([AccountId], [ExpiresAt]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_AccountRefreshTokens_TokenHash'
                      AND object_id = OBJECT_ID(N'[AccountRefreshTokens]')
                )
                    CREATE UNIQUE INDEX [IX_AccountRefreshTokens_TokenHash]
                    ON [AccountRefreshTokens] ([TokenHash]);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StaffUsers_AccountId",
                table: "StaffUsers",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residents_AccountId",
                table: "Residents",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OwnerAccountId",
                table: "Organizations",
                column: "OwnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Phone",
                table: "Accounts",
                column: "Phone",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAccessTokens_Accounts_AccountId",
                table: "AccountAccessTokens",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRefreshTokens_Accounts_AccountId",
                table: "AccountRefreshTokens",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Accounts_OwnerAccountId",
                table: "Organizations",
                column: "OwnerAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_Accounts_AccountId",
                table: "Residents",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffUsers_Accounts_AccountId",
                table: "StaffUsers",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountAccessTokens_Accounts_AccountId",
                table: "AccountAccessTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountRefreshTokens_Accounts_AccountId",
                table: "AccountRefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Accounts_OwnerAccountId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Residents_Accounts_AccountId",
                table: "Residents");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffUsers_Accounts_AccountId",
                table: "StaffUsers");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_StaffUsers_AccountId",
                table: "StaffUsers");

            migrationBuilder.DropIndex(
                name: "IX_Residents_AccountId",
                table: "Residents");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_OwnerAccountId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "StaffUsers");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "OwnerAccountId",
                table: "Organizations");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "AccountRefreshTokens",
                newName: "StaffUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AccountRefreshTokens_AccountId_ExpiresAt",
                table: "AccountRefreshTokens",
                newName: "IX_AccountRefreshTokens_UserId_ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "AccountAccessTokens",
                newName: "StaffUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AccountAccessTokens_AccountId_ExpiresAt",
                table: "AccountAccessTokens",
                newName: "IX_AccountAccessTokens_UserId_ExpiresAt");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffUsers",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "StaffUsers",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "StaffUsers",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAccessTokens_Users_UserId",
                table: "AccountAccessTokens",
                column: "StaffUserId",
                principalTable: "StaffUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRefreshTokens_Users_UserId",
                table: "AccountRefreshTokens",
                column: "StaffUserId",
                principalTable: "StaffUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
