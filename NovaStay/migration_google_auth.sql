IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Accounts] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [AccountType] varchar(30) NOT NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [Email] varchar(100) NULL,
        [Phone] varchar(15) NOT NULL,
        [PasswordHash] varchar(255) NULL,
        [MustSetPassword] bit NULL DEFAULT CAST(0 AS bit),
        [PasswordSetAt] datetime NULL,
        [IsActive] bit NULL DEFAULT CAST(1 AS bit),
        [LastLoginAt] datetime NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PermissionName] nvarchar(100) NOT NULL,
        [PermissionKey] varchar(100) NOT NULL,
        [Module] varchar(50) NOT NULL,
        CONSTRAINT [PK__Permissi__3214EC0720D981B2] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [SubscriptionPackages] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PackageName] nvarchar(50) NOT NULL,
        [PackageKey] varchar(20) NOT NULL,
        [PriceMonthly] decimal(18,2) NOT NULL,
        [MaxProperties] int NOT NULL,
        [MaxRooms] int NOT NULL,
        [TokenGiftMonthly] int NULL DEFAULT 0,
        [AllowCustomRoles] bit NULL DEFAULT CAST(0 AS bit),
        [AllowAiFeatures] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Subscrip__3214EC077D1660D7] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
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
        CONSTRAINT [PK_AccountRefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AccountRefreshTokens_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Residents] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [AccountId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Sex] nvarchar(20) NOT NULL,
        [Phone] varchar(15) NOT NULL,
        [Email] varchar(100) NULL,
        [Address] nvarchar(255) NULL,
        [IdentityCardNumber] varchar(20) NULL,
        [IdFrontImageUrl] varchar(255) NULL,
        [IdBackImageUrl] varchar(255) NULL,
        [ProfileImageUrl] varchar(255) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Resident__3214EC07115F9503] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Residents_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Organizations] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OwnerAccountId] uniqueidentifier NOT NULL,
        [PackageId] uniqueidentifier NOT NULL,
        [BusinessName] nvarchar(150) NOT NULL,
        [BusinessArea] nvarchar(100) NOT NULL,
        [TaxCode] varchar(20) NULL,
        [OwnerEmail] varchar(100) NOT NULL,
        [OwnerPhone] varchar(15) NOT NULL,
        [SubscriptionStatus] varchar(20) NOT NULL,
        [TokenBalance] int NULL DEFAULT 0,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK__Organizations__3214EC070623D35C] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Organizations_Accounts_OwnerAccountId] FOREIGN KEY ([OwnerAccountId]) REFERENCES [Accounts] ([Id]),
        CONSTRAINT [FK__Organizations__Package__534D60F1] FOREIGN KEY ([PackageId]) REFERENCES [SubscriptionPackages] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Assets] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [AssetName] nvarchar(100) NOT NULL,
        [Category] nvarchar(50) NULL,
        [Brand] nvarchar(50) NULL,
        [Model] varchar(50) NULL,
        [AssetCode] varchar(50) NULL,
        [PurchaseDate] date NULL,
        [WarrantyExpiryDate] date NULL,
        [BaseValue] decimal(18,2) NULL DEFAULT 0.0,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK__Assets__3214EC071B1A8B14] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Assets__OrganizationId__02FC7413] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Brokers] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Phone] varchar(15) NOT NULL,
        [WalletBalance] decimal(18,2) NULL DEFAULT 0.0,
        [TotalCommissionEarned] decimal(18,2) NULL DEFAULT 0.0,
        CONSTRAINT [PK__Brokers__3214EC07D3AA9FC2] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Brokers__OrganizationI__2739D489] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationServices] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [ServiceName] nvarchar(150) NOT NULL,
        [ServiceCode] varchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [DefaultPrice] decimal(18,2) NOT NULL,
        [Unit] nvarchar(50) NULL,
        [BillingCycle] varchar(30) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK_OrganizationServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationServices_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Properties] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [PropertyName] nvarchar(150) NOT NULL,
        [Address] nvarchar(255) NOT NULL,
        [PropertyType] varchar(20) NOT NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Properti__3214EC07FE441D45] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Propertie__Tenan__6A30C649] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [ResidentMemberships] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [AccountId] uniqueidentifier NOT NULL,
        [ResidentId] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [MembershipCode] varchar(20) NOT NULL,
        [Status] varchar(20) NOT NULL DEFAULT 'Pending',
        [InvitedAt] datetime NULL DEFAULT ((getdate())),
        [RespondedAt] datetime NULL,
        [JoinedAt] datetime NULL DEFAULT ((getdate())),
        [ActivatedAt] datetime NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_ResidentMemberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResidentMemberships_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]),
        CONSTRAINT [FK_ResidentMemberships_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK_ResidentMemberships_Residents_ResidentId] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [RoleKey] varchar(50) NOT NULL,
        [Description] nvarchar(255) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Roles__3214EC07BE72DF6E] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Roles__OrganizationId__5BE2A6F2] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffUsers] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [AccountId] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [IsActive] bit NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Users__3214EC078909D83A] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffUsers_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]),
        CONSTRAINT [FK__Users__OrganizationId__5812160E] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Technicians] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Phone] varchar(15) NOT NULL,
        [Specialty] nvarchar(50) NOT NULL,
        [IsAvailable] bit NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK__Technici__3214EC0701E593D1] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Technicia__Tenan__3D2915A8] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PropertyId] uniqueidentifier NOT NULL,
        [RoomNumber] varchar(20) NOT NULL,
        [Floor] int NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [MaxOccupants] int NULL DEFAULT 1,
        [AmenitiesJson] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK__Rooms__3214EC077FA99C97] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Rooms__PropertyI__72C60C4A] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK__RolePerm__6400A1A8CB2E4434] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK__RolePermi__Permi__628FA481] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK__RolePermi__RoleI__619B8048] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffUserPropertyMapping] (
        [StaffUserId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK__UserProp__5084563FC247FEEC] PRIMARY KEY ([StaffUserId], [PropertyId]),
        CONSTRAINT [FK__UserPrope__Prope__6E01572D] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK__UserPrope__UserI__6D0D32F4] FOREIGN KEY ([StaffUserId]) REFERENCES [StaffUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffUserRoles] (
        [StaffUserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK__UserRole__AF2760AD29238DE0] PRIMARY KEY ([StaffUserId], [RoleId]),
        CONSTRAINT [FK__StaffUserRoles__RoleI__66603565] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK__StaffUserRoles__UserI__656C112C] FOREIGN KEY ([StaffUserId]) REFERENCES [StaffUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [AssetAssignments] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [AssetId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NULL,
        [Status] varchar(20) NULL DEFAULT 'Good',
        [Note] nvarchar(255) NULL,
        [AssignedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__AssetAss__3214EC073B842B58] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__AssetAssi__Asset__08B54D69] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK__AssetAssi__RoomI__09A971A2] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]),
        CONSTRAINT [FK__AssetAssi__Tenan__07C12930] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [GuestName] nvarchar(100) NOT NULL,
        [GuestPhone] varchar(15) NOT NULL,
        [GuestEmail] varchar(100) NULL,
        [BookingType] varchar(20) NOT NULL,
        [CheckInDate] date NOT NULL,
        [CheckOutDate] date NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentStatus] varchar(20) NULL DEFAULT 'Pending',
        [PaymentTransactionId] varchar(100) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Bookings__3214EC07E0378A72] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Bookings__Organization__1BC821DD] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK__Bookings__Proper__1CBC4616] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK__Bookings__RoomId__1DB06A4F] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomAvailabilities] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [RoomId] uniqueidentifier NOT NULL,
        [StayDate] date NOT NULL,
        [DynamicPrice] decimal(18,2) NOT NULL,
        [IsBooked] bit NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK__RoomAvai__3214EC07207FCE01] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__RoomAvail__RoomI__7E37BEF6] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomImages] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [RoomId] uniqueidentifier NOT NULL,
        [ImageUrl] varchar(255) NOT NULL,
        [IsCover] bit NULL DEFAULT CAST(0 AS bit),
        [UploadedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__RoomImag__3214EC079F9973C0] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__RoomImage__RoomI__778AC167] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomServices] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [RoomId] uniqueidentifier NOT NULL,
        [OrganizationServiceId] uniqueidentifier NOT NULL,
        [PriceOverride] decimal(18,2) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AssignedAt] datetime NULL DEFAULT ((getdate())),
        [RemovedAt] datetime NULL,
        [Note] nvarchar(255) NULL,
        CONSTRAINT [PK_RoomServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoomServices_OrganizationServices_OrganizationServiceId] FOREIGN KEY ([OrganizationServiceId]) REFERENCES [OrganizationServices] ([Id]),
        CONSTRAINT [FK_RoomServices_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [MaintenanceTickets] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [ResidentId] uniqueidentifier NOT NULL,
        [AssetAssignmentId] uniqueidentifier NULL,
        [Category] nvarchar(50) NOT NULL,
        [UserDescription] nvarchar(500) NOT NULL,
        [IncidentImageUrl] varchar(255) NULL,
        [Status] varchar(20) NULL DEFAULT 'Pending',
        [TechnicianId] uniqueidentifier NULL,
        [ResolvedImageUrl] varchar(255) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK__Maintena__3214EC071D5966C6] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Maintenan__Asset__44CA3770] FOREIGN KEY ([AssetAssignmentId]) REFERENCES [AssetAssignments] ([Id]),
        CONSTRAINT [FK__Maintenan__Resid__43D61337] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id]),
        CONSTRAINT [FK__Maintenan__RoomI__42E1EEFE] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]),
        CONSTRAINT [FK__Maintenan__Techn__45BE5BA9] FOREIGN KEY ([TechnicianId]) REFERENCES [Technicians] ([Id]),
        CONSTRAINT [FK__Maintenan__Tenan__41EDCAC5] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Contracts] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [ResidentId] uniqueidentifier NOT NULL,
        [BrokerId] uniqueidentifier NULL,
        [BookingId] uniqueidentifier NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [DepositAmount] decimal(18,2) NOT NULL,
        [BrokerCommission] decimal(18,2) NULL DEFAULT 0.0,
        [CommissionStatus] varchar(20) NULL DEFAULT 'Pending',
        [ContractPdfUrl] varchar(255) NULL,
        [Status] varchar(20) NOT NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Contract__3214EC0707402DA0] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Contracts__Booki__32AB8735] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]),
        CONSTRAINT [FK__Contracts__Broke__31B762FC] FOREIGN KEY ([BrokerId]) REFERENCES [Brokers] ([Id]),
        CONSTRAINT [FK__Contracts__Prope__2DE6D218] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK__Contracts__Resid__30C33EC3] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id]),
        CONSTRAINT [FK__Contracts__RoomI__2EDAF651] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]),
        CONSTRAINT [FK__Contracts__Tenan__2CF2ADDF] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [ContractId] uniqueidentifier NULL,
        [BookingId] uniqueidentifier NULL,
        [InvoicePeriod] varchar(7) NOT NULL,
        [RoomPrice] decimal(18,2) NOT NULL,
        [ServicesPrice] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [QrCodeUrl] varchar(500) NULL,
        [Status] varchar(20) NULL DEFAULT 'Unpaid',
        [PaidAt] datetime NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Invoices__3214EC07023BFFC1] PRIMARY KEY ([Id]),
        CONSTRAINT [FK__Invoices__Bookin__395884C4] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]),
        CONSTRAINT [FK__Invoices__Contra__3864608B] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]),
        CONSTRAINT [FK__Invoices__Organization__37703C52] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccountRefreshTokens_AccountId_ExpiresAt] ON [AccountRefreshTokens] ([AccountId], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AccountRefreshTokens_TokenHash] ON [AccountRefreshTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Accounts_Email] ON [Accounts] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Accounts_Phone] ON [Accounts] ([Phone]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetAssignments_AssetId] ON [AssetAssignments] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetAssignments_OrganizationId] ON [AssetAssignments] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetAssignments_RoomId] ON [AssetAssignments] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_OrganizationId] ON [Assets] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_OrganizationId] ON [Bookings] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_PropertyId] ON [Bookings] ([PropertyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_RoomId] ON [Bookings] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Brokers_OrganizationId] ON [Brokers] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_BookingId] ON [Contracts] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_BrokerId] ON [Contracts] ([BrokerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_OrganizationId] ON [Contracts] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_PropertyId] ON [Contracts] ([PropertyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_ResidentId] ON [Contracts] ([ResidentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_RoomId] ON [Contracts] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_BookingId] ON [Invoices] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_ContractId] ON [Invoices] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_OrganizationId] ON [Invoices] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_Status_Period] ON [Invoices] ([Status], [InvoicePeriod]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceTickets_AssetAssignmentId] ON [MaintenanceTickets] ([AssetAssignmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceTickets_OrganizationId] ON [MaintenanceTickets] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceTickets_ResidentId] ON [MaintenanceTickets] ([ResidentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceTickets_RoomId] ON [MaintenanceTickets] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceTickets_TechnicianId] ON [MaintenanceTickets] ([TechnicianId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Organizations_OwnerAccountId] ON [Organizations] ([OwnerAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Organizations_PackageId] ON [Organizations] ([PackageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__Organizations__FF0186BBAE6B6868] ON [Organizations] ([OwnerEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationServices_OrganizationId_IsActive] ON [OrganizationServices] ([OrganizationId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_OrganizationServices_OrganizationId_ServiceName] ON [OrganizationServices] ([OrganizationId], [ServiceName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__Permissi__8884ABD46702546C] ON [Permissions] ([PermissionKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Properties_OrganizationId] ON [Properties] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResidentMemberships_AccountId] ON [ResidentMemberships] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResidentMemberships_OrganizationId] ON [ResidentMemberships] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResidentMemberships_ResidentId] ON [ResidentMemberships] ([ResidentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ResidentMemberships_AccountId_OrganizationId] ON [ResidentMemberships] ([AccountId], [OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ResidentMemberships_MembershipCode] ON [ResidentMemberships] ([MembershipCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Residents_AccountId] ON [Residents] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Roles_OrganizationId] ON [Roles] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomAvailabilities_RoomId] ON [RoomAvailabilities] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomImages_RoomId] ON [RoomImages] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rooms_PropertyId_Status] ON [Rooms] ([PropertyId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomServices_OrganizationServiceId] ON [RoomServices] ([OrganizationServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoomServices_RoomId] ON [RoomServices] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_RoomServices_RoomId_OrganizationServiceId] ON [RoomServices] ([RoomId], [OrganizationServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StaffUserPropertyMapping_PropertyId] ON [StaffUserPropertyMapping] ([PropertyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StaffUserRoles_RoleId] ON [StaffUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffUsers_AccountId] ON [StaffUsers] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_OrganizationId] ON [StaffUsers] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__Subscrip__63953E72B6E7DAB9] ON [SubscriptionPackages] ([PackageKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Technicians_OrganizationId] ON [Technicians] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626145728_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260626145728_InitialCreate', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    ALTER TABLE [RoomServices] DROP CONSTRAINT [FK_RoomServices_OrganizationServices_OrganizationServiceId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DROP INDEX [UX_RoomServices_RoomId_OrganizationServiceId] ON [RoomServices];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DROP INDEX [IX_RoomServices_OrganizationServiceId] ON [RoomServices];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    CREATE TABLE [PropertyServices] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PropertyId] uniqueidentifier NOT NULL,
        [ServiceName] nvarchar(150) NOT NULL,
        [ServiceCode] varchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [DefaultPrice] decimal(18,2) NOT NULL,
        [Unit] nvarchar(50) NULL,
        [BillingCycle] varchar(30) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        [OldOrganizationServiceId] uniqueidentifier NULL,
        CONSTRAINT [PK_PropertyServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PropertyServices_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    INSERT INTO PropertyServices (
        Id,
        PropertyId,
        ServiceName,
        ServiceCode,
        Description,
        DefaultPrice,
        Unit,
        BillingCycle,
        IsActive,
        CreatedAt,
        UpdatedAt,
        OldOrganizationServiceId)
    SELECT
        NEWID(),
        p.Id,
        os.ServiceName,
        os.ServiceCode,
        os.Description,
        os.DefaultPrice,
        os.Unit,
        os.BillingCycle,
        os.IsActive,
        os.CreatedAt,
        os.UpdatedAt,
        os.Id
    FROM OrganizationServices os
    INNER JOIN Properties p ON p.OrganizationId = os.OrganizationId;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    ALTER TABLE [RoomServices] ADD [PropertyServiceId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    UPDATE rs
    SET PropertyServiceId = ps.Id
    FROM RoomServices rs
    INNER JOIN Rooms r ON r.Id = rs.RoomId
    INNER JOIN PropertyServices ps
        ON ps.PropertyId = r.PropertyId
        AND ps.OldOrganizationServiceId = rs.OrganizationServiceId;

    DELETE FROM RoomServices
    WHERE PropertyServiceId IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RoomServices]') AND [c].[name] = N'OrganizationServiceId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [RoomServices] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [RoomServices] DROP COLUMN [OrganizationServiceId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DROP TABLE [OrganizationServices];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PropertyServices]') AND [c].[name] = N'OldOrganizationServiceId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PropertyServices] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [PropertyServices] DROP COLUMN [OldOrganizationServiceId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RoomServices]') AND [c].[name] = N'PropertyServiceId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RoomServices] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [RoomServices] ALTER COLUMN [PropertyServiceId] uniqueidentifier NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    CREATE INDEX [IX_PropertyServices_PropertyId_IsActive] ON [PropertyServices] ([PropertyId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PropertyServices_PropertyId_ServiceName] ON [PropertyServices] ([PropertyId], [ServiceName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    CREATE INDEX [IX_RoomServices_PropertyServiceId] ON [RoomServices] ([PropertyServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    CREATE UNIQUE INDEX [UX_RoomServices_RoomId_PropertyServiceId] ON [RoomServices] ([RoomId], [PropertyServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    ALTER TABLE [RoomServices] ADD CONSTRAINT [FK_RoomServices_PropertyServices_PropertyServiceId] FOREIGN KEY ([PropertyServiceId]) REFERENCES [PropertyServices] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626153559_MoveServicesToProperty'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260626153559_MoveServicesToProperty', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    DROP INDEX [IX_Invoices_OrganizationId] ON [Invoices];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [AdjustmentAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [AmountPaid] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [DueDate] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [InvoiceNumber] varchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [InvoiceType] varchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [IssuedAt] datetime NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [LateFeeAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [Note] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [OutstandingAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PropertyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [ResidentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [RoomId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [Subtotal] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD [UpdatedAt] datetime NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE TABLE [ExpenseCategories] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [CategoryCode] varchar(30) NOT NULL,
        [CategoryName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExpenseCategories_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE TABLE [InvoiceLines] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [InvoiceId] uniqueidentifier NOT NULL,
        [LineType] varchar(30) NOT NULL,
        [SourceType] varchar(30) NULL,
        [SourceId] uniqueidentifier NULL,
        [Description] nvarchar(255) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL DEFAULT 1.0,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineAmount] decimal(18,2) NOT NULL,
        [BillingStartDate] date NULL,
        [BillingEndDate] date NULL,
        [DisplayOrder] int NOT NULL,
        [IsDebit] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_InvoiceLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceLines_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE TABLE [PaymentReceipts] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NULL,
        [ResidentId] uniqueidentifier NULL,
        [CollectedByStaffUserId] uniqueidentifier NULL,
        [ReceiptNumber] varchar(30) NOT NULL,
        [ReceiptType] varchar(20) NOT NULL,
        [PaymentMethod] varchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaidAt] datetime NOT NULL,
        [ReferenceCode] varchar(100) NULL,
        [Status] varchar(20) NOT NULL,
        [PayerName] nvarchar(100) NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_PaymentReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentReceipts_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK_PaymentReceipts_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK_PaymentReceipts_Residents_ResidentId] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id]),
        CONSTRAINT [FK_PaymentReceipts_StaffUsers_CollectedByStaffUserId] FOREIGN KEY ([CollectedByStaffUserId]) REFERENCES [StaffUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE TABLE [Expenses] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [ExpenseCategoryId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NULL,
        [RoomId] uniqueidentifier NULL,
        [RelatedMaintenanceTicketId] uniqueidentifier NULL,
        [RelatedBrokerId] uniqueidentifier NULL,
        [ApprovedByStaffUserId] uniqueidentifier NULL,
        [CreatedByStaffUserId] uniqueidentifier NULL,
        [ExpenseNumber] varchar(30) NOT NULL,
        [ExpenseType] varchar(20) NOT NULL,
        [PayeeName] nvarchar(150) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [SpentAt] datetime NOT NULL,
        [PaymentMethod] varchar(20) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [ReferenceCode] varchar(100) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Expenses_Brokers_RelatedBrokerId] FOREIGN KEY ([RelatedBrokerId]) REFERENCES [Brokers] ([Id]),
        CONSTRAINT [FK_Expenses_ExpenseCategories_ExpenseCategoryId] FOREIGN KEY ([ExpenseCategoryId]) REFERENCES [ExpenseCategories] ([Id]),
        CONSTRAINT [FK_Expenses_MaintenanceTickets_RelatedMaintenanceTicketId] FOREIGN KEY ([RelatedMaintenanceTicketId]) REFERENCES [MaintenanceTickets] ([Id]),
        CONSTRAINT [FK_Expenses_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK_Expenses_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK_Expenses_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]),
        CONSTRAINT [FK_Expenses_StaffUsers_ApprovedByStaffUserId] FOREIGN KEY ([ApprovedByStaffUserId]) REFERENCES [StaffUsers] ([Id]),
        CONSTRAINT [FK_Expenses_StaffUsers_CreatedByStaffUserId] FOREIGN KEY ([CreatedByStaffUserId]) REFERENCES [StaffUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE TABLE [PaymentAllocations] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PaymentReceiptId] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NOT NULL,
        [AllocatedAmount] decimal(18,2) NOT NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_PaymentAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentAllocations_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]),
        CONSTRAINT [FK_PaymentAllocations_PaymentReceipts_PaymentReceiptId] FOREIGN KEY ([PaymentReceiptId]) REFERENCES [PaymentReceipts] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Invoices_PropertyId_RoomId_Period] ON [Invoices] ([PropertyId], [RoomId], [InvoicePeriod]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Invoices_ResidentId_Status] ON [Invoices] ([ResidentId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Invoices_RoomId] ON [Invoices] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Invoices_OrganizationId_InvoiceNumber] ON [Invoices] ([OrganizationId], [InvoiceNumber]) WHERE [InvoiceNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ExpenseCategories_OrganizationId_CategoryCode] ON [ExpenseCategories] ([OrganizationId], [CategoryCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_ApprovedByStaffUserId] ON [Expenses] ([ApprovedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_CreatedByStaffUserId] ON [Expenses] ([CreatedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_ExpenseCategoryId] ON [Expenses] ([ExpenseCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_OrganizationId_SpentAt] ON [Expenses] ([OrganizationId], [SpentAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_PropertyId_ExpenseCategoryId_SpentAt] ON [Expenses] ([PropertyId], [ExpenseCategoryId], [SpentAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_RelatedBrokerId] ON [Expenses] ([RelatedBrokerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_RelatedMaintenanceTicketId] ON [Expenses] ([RelatedMaintenanceTicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_Expenses_RoomId] ON [Expenses] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Expenses_OrganizationId_ExpenseNumber] ON [Expenses] ([OrganizationId], [ExpenseNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_InvoiceId_DisplayOrder] ON [InvoiceLines] ([InvoiceId], [DisplayOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_InvoiceId] ON [PaymentAllocations] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentAllocations_PaymentReceiptId_InvoiceId] ON [PaymentAllocations] ([PaymentReceiptId], [InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_CollectedByStaffUserId] ON [PaymentReceipts] ([CollectedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_OrganizationId_PaidAt] ON [PaymentReceipts] ([OrganizationId], [PaidAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_PropertyId] ON [PaymentReceipts] ([PropertyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_ResidentId] ON [PaymentReceipts] ([ResidentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentReceipts_OrganizationId_ReceiptNumber] ON [PaymentReceipts] ([OrganizationId], [ReceiptNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Residents_ResidentId] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260627155419_20260627_AddFinancialManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260627155419_20260627_AddFinancialManagement', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628012820_MakeResidentIdNullableInMaintenanceTickets'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MaintenanceTickets]') AND [c].[name] = N'ResidentId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [MaintenanceTickets] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [MaintenanceTickets] ALTER COLUMN [ResidentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628012820_MakeResidentIdNullableInMaintenanceTickets'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260628012820_MakeResidentIdNullableInMaintenanceTickets', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE TABLE [UtilityMeters] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [RoomId] uniqueidentifier NOT NULL,
        [PropertyServiceId] uniqueidentifier NOT NULL,
        [MeterCode] varchar(50) NULL,
        [MeterType] varchar(30) NOT NULL,
        [Unit] nvarchar(20) NOT NULL,
        [InitialReading] decimal(18,3) NOT NULL,
        [InstalledAt] datetime NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK_UtilityMeters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UtilityMeters_PropertyServices_PropertyServiceId] FOREIGN KEY ([PropertyServiceId]) REFERENCES [PropertyServices] ([Id]),
        CONSTRAINT [FK_UtilityMeters_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE TABLE [UtilityTariffs] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [PropertyServiceId] uniqueidentifier NOT NULL,
        [EffectiveFrom] datetime NOT NULL,
        [EffectiveTo] datetime NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [PricingMode] varchar(20) NOT NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_UtilityTariffs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UtilityTariffs_PropertyServices_PropertyServiceId] FOREIGN KEY ([PropertyServiceId]) REFERENCES [PropertyServices] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE TABLE [UtilityReadings] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [UtilityMeterId] uniqueidentifier NOT NULL,
        [ReadingDate] datetime NOT NULL,
        [BillingPeriod] varchar(7) NOT NULL,
        [PreviousReading] decimal(18,3) NOT NULL,
        [CurrentReading] decimal(18,3) NOT NULL,
        [Consumption] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] varchar(20) NOT NULL DEFAULT 'Draft',
        [RecordedByStaffUserId] uniqueidentifier NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_UtilityReadings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UtilityReadings_StaffUsers_RecordedByStaffUserId] FOREIGN KEY ([RecordedByStaffUserId]) REFERENCES [StaffUsers] ([Id]),
        CONSTRAINT [FK_UtilityReadings_UtilityMeters_UtilityMeterId] FOREIGN KEY ([UtilityMeterId]) REFERENCES [UtilityMeters] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE INDEX [IX_UtilityMeters_PropertyServiceId] ON [UtilityMeters] ([PropertyServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE INDEX [IX_UtilityMeters_RoomId] ON [UtilityMeters] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_UtilityMeters_MeterCode] ON [UtilityMeters] ([MeterCode]) WHERE [MeterCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE UNIQUE INDEX [UX_UtilityMeters_RoomId_PropertyServiceId] ON [UtilityMeters] ([RoomId], [PropertyServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE INDEX [IX_UtilityReadings_RecordedByStaffUserId] ON [UtilityReadings] ([RecordedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE INDEX [IX_UtilityReadings_Status_BillingPeriod] ON [UtilityReadings] ([Status], [BillingPeriod]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE UNIQUE INDEX [UX_UtilityReadings_UtilityMeterId_BillingPeriod] ON [UtilityReadings] ([UtilityMeterId], [BillingPeriod]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    CREATE UNIQUE INDEX [UX_UtilityTariffs_PropertyServiceId_EffectiveFrom] ON [UtilityTariffs] ([PropertyServiceId], [EffectiveFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629013739_AddUtilityMetering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629013739_AddUtilityMetering', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    CREATE TABLE [InvoiceGenerationSchedules] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NULL,
        [ScheduleName] nvarchar(150) NOT NULL,
        [BillingCycle] varchar(20) NOT NULL,
        [GenerateDayOfMonth] int NOT NULL,
        [GenerateTime] time NOT NULL,
        [TimeZone] nvarchar(100) NOT NULL,
        [DueAfterDays] int NOT NULL DEFAULT 7,
        [AutoSendToResident] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [LastRunAt] datetime NULL,
        [NextRunAt] datetime NULL,
        [CreatedByStaffUserId] uniqueidentifier NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK_InvoiceGenerationSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceGenerationSchedules_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK_InvoiceGenerationSchedules_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK_InvoiceGenerationSchedules_StaffUsers_CreatedByStaffUserId] FOREIGN KEY ([CreatedByStaffUserId]) REFERENCES [StaffUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    CREATE INDEX [IX_InvoiceGenerationSchedules_CreatedByStaffUserId] ON [InvoiceGenerationSchedules] ([CreatedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    CREATE INDEX [IX_InvoiceGenerationSchedules_IsActive_NextRunAt] ON [InvoiceGenerationSchedules] ([IsActive], [NextRunAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    CREATE INDEX [IX_InvoiceGenerationSchedules_OrganizationId] ON [InvoiceGenerationSchedules] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    CREATE INDEX [IX_InvoiceGenerationSchedules_PropertyId] ON [InvoiceGenerationSchedules] ([PropertyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_InvoiceGenerationSchedules_OrganizationId_ScheduleName] ON [InvoiceGenerationSchedules] ([OrganizationId], [ScheduleName]) WHERE [PropertyId] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_InvoiceGenerationSchedules_OrganizationId_PropertyId_ScheduleName] ON [InvoiceGenerationSchedules] ([OrganizationId], [PropertyId], [ScheduleName]) WHERE [PropertyId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629021022_AddInvoiceGenerationSchedules'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629021022_AddInvoiceGenerationSchedules', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [InvoiceId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM PaymentAllocations
        GROUP BY PaymentReceiptId
        HAVING COUNT(DISTINCT InvoiceId) > 1
    )
    BEGIN
        THROW 51000, 'Cannot simplify invoice payments because at least one payment receipt is allocated to multiple invoices.', 1;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    UPDATE pr
    SET pr.InvoiceId = pa.InvoiceId
    FROM PaymentReceipts pr
    INNER JOIN (
        SELECT PaymentReceiptId, MIN(InvoiceId) AS InvoiceId
        FROM PaymentAllocations
        GROUP BY PaymentReceiptId
    ) pa ON pa.PaymentReceiptId = pr.Id
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM PaymentReceipts
        WHERE InvoiceId IS NULL
    )
    BEGIN
        THROW 51001, 'Cannot simplify invoice payments because at least one payment receipt is not linked to an invoice.', 1;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    UPDATE i
    SET
        i.Status = 'Paid',
        i.PaidAt = COALESCE(pr.PaidAt, i.PaidAt)
    FROM Invoices i
    INNER JOIN PaymentReceipts pr ON pr.InvoiceId = i.Id
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentReceipts]') AND [c].[name] = N'InvoiceId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [PaymentReceipts] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [PaymentReceipts] ALTER COLUMN [InvoiceId] uniqueidentifier NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentReceipts_InvoiceId] ON [PaymentReceipts] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD CONSTRAINT [FK_PaymentReceipts_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    DROP TABLE [PaymentAllocations];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invoices]') AND [c].[name] = N'AmountPaid');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Invoices] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Invoices] DROP COLUMN [AmountPaid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invoices]') AND [c].[name] = N'OutstandingAmount');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Invoices] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Invoices] DROP COLUMN [OutstandingAmount];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629030045_SimplifyInvoicePayments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629030045_SimplifyInvoicePayments', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629045111_AddPropertyStatus'
)
BEGIN
    ALTER TABLE [Properties] ADD [Status] varchar(20) NOT NULL DEFAULT 'Active';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629045111_AddPropertyStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629045111_AddPropertyStatus', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE TABLE [IncomeCategories] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [CategoryCode] varchar(30) NOT NULL,
        [CategoryName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_IncomeCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IncomeCategories_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE TABLE [IncomeReceipts] (
        [Id] uniqueidentifier NOT NULL DEFAULT ((newsequentialid())),
        [OrganizationId] uniqueidentifier NOT NULL,
        [IncomeCategoryId] uniqueidentifier NOT NULL,
        [PropertyId] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NULL,
        [ResidentId] uniqueidentifier NULL,
        [CollectedByStaffUserId] uniqueidentifier NULL,
        [ReceiptNumber] varchar(30) NOT NULL,
        [IncomeType] varchar(30) NOT NULL,
        [PayerName] nvarchar(150) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [CollectedAt] datetime NOT NULL,
        [PaymentMethod] varchar(20) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [ReferenceCode] varchar(100) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime NULL DEFAULT ((getdate())),
        [UpdatedAt] datetime NULL,
        CONSTRAINT [PK_IncomeReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IncomeReceipts_IncomeCategories_IncomeCategoryId] FOREIGN KEY ([IncomeCategoryId]) REFERENCES [IncomeCategories] ([Id]),
        CONSTRAINT [FK_IncomeReceipts_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
        CONSTRAINT [FK_IncomeReceipts_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([Id]),
        CONSTRAINT [FK_IncomeReceipts_Residents_ResidentId] FOREIGN KEY ([ResidentId]) REFERENCES [Residents] ([Id]),
        CONSTRAINT [FK_IncomeReceipts_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]),
        CONSTRAINT [FK_IncomeReceipts_StaffUsers_CollectedByStaffUserId] FOREIGN KEY ([CollectedByStaffUserId]) REFERENCES [StaffUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE UNIQUE INDEX [UX_IncomeCategories_OrganizationId_CategoryCode] ON [IncomeCategories] ([OrganizationId], [CategoryCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_CollectedByStaffUserId] ON [IncomeReceipts] ([CollectedByStaffUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_IncomeCategoryId] ON [IncomeReceipts] ([IncomeCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_OrganizationId_CollectedAt] ON [IncomeReceipts] ([OrganizationId], [CollectedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_PropertyId_IncomeCategoryId_CollectedAt] ON [IncomeReceipts] ([PropertyId], [IncomeCategoryId], [CollectedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_ResidentId] ON [IncomeReceipts] ([ResidentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE INDEX [IX_IncomeReceipts_RoomId] ON [IncomeReceipts] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    CREATE UNIQUE INDEX [UX_IncomeReceipts_OrganizationId_ReceiptNumber] ON [IncomeReceipts] ([OrganizationId], [ReceiptNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629114314_AddManualIncomeReceipts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629114314_AddManualIncomeReceipts', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019163812_AddGoogleAuthFields'
)
BEGIN
    ALTER TABLE [Accounts] ADD [ExternalProvider] varchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019163812_AddGoogleAuthFields'
)
BEGIN
    ALTER TABLE [Accounts] ADD [GoogleId] varchar(128) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019163812_AddGoogleAuthFields'
)
BEGIN
    ALTER TABLE [Accounts] ADD [IsEmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019163812_AddGoogleAuthFields'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Accounts_GoogleId] ON [Accounts] ([GoogleId]) WHERE [GoogleId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019163812_AddGoogleAuthFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019163812_AddGoogleAuthFields', N'8.0.28');
END;
GO

COMMIT;
GO

