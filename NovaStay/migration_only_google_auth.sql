BEGIN TRANSACTION;
GO

ALTER TABLE [Accounts] ADD [ExternalProvider] varchar(50) NULL;
GO

ALTER TABLE [Accounts] ADD [GoogleId] varchar(128) NULL;
GO

ALTER TABLE [Accounts] ADD [IsEmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE UNIQUE INDEX [IX_Accounts_GoogleId] ON [Accounts] ([GoogleId]) WHERE [GoogleId] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261019163812_AddGoogleAuthFields', N'8.0.28');
GO

COMMIT;
GO

