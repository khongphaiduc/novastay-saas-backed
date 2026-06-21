using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [SubscriptionPackages] ([Id], [PackageName], [PackageKey], [PriceMonthly], [MaxProperties], [MaxRooms], [TokenGiftMonthly], [AllowCustomRoles], [AllowAiFeatures], [CreatedAt])
                VALUES
                ('11111111-1111-1111-1111-111111111111', N'NovaStay Starter', 'STARTER', 299000.00, 3, 30, 100, 1, 0, '2026-06-20T09:00:00'),
                ('11111111-1111-1111-1111-111111111112', N'NovaStay Pro', 'PRO', 799000.00, 15, 200, 500, 1, 1, '2026-06-20T09:00:00');

                INSERT INTO [Permissions] ([Id], [PermissionName], [PermissionKey], [Module])
                VALUES
                ('22222222-2222-2222-2222-222222222221', N'View properties', 'properties.view', 'properties'),
                ('22222222-2222-2222-2222-222222222222', N'Manage rooms', 'rooms.manage', 'rooms'),
                ('22222222-2222-2222-2222-222222222223', N'Manage contracts', 'contracts.manage', 'contracts'),
                ('22222222-2222-2222-2222-222222222224', N'Manage invoices', 'invoices.manage', 'invoices');

                INSERT INTO [Organizations] ([Id], [PackageId], [BusinessName], [TaxCode], [OwnerEmail], [OwnerPhone], [SubscriptionStatus], [TokenBalance], [CreatedAt], [UpdatedAt])
                VALUES
                ('33333333-3333-3333-3333-333333333331', '11111111-1111-1111-1111-111111111112', N'NovaStay Demo Operator', '0312345678', 'owner.demo@novastay.local', '0901000001', 'Active', 450, '2026-06-20T09:05:00', '2026-06-20T09:05:00');

                INSERT INTO [Roles] ([Id], [OrganizationId], [RoleName], [RoleKey], [Description], [CreatedAt])
                VALUES
                ('44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331', N'Property Manager', 'PROPERTY_MANAGER', N'Manages properties, rooms, contracts, and invoices.', '2026-06-20T09:10:00');

                INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
                VALUES
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222221'),
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222222'),
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222223'),
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222224');

                INSERT INTO [StaffUsers] ([Id], [OrganizationId], [FullName], [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt])
                VALUES
                ('55555555-5555-5555-5555-555555555551', '33333333-3333-3333-3333-333333333331', N'Demo Manager', 'manager.demo@novastay.local', '0901000002', 'sample-password-hash-change-me', 1, '2026-06-20T09:15:00');

                INSERT INTO [StaffUserRoles] ([StaffUserId], [RoleId])
                VALUES
                ('55555555-5555-5555-5555-555555555551', '44444444-4444-4444-4444-444444444441');

                INSERT INTO [Properties] ([Id], [OrganizationId], [PropertyName], [Address], [PropertyType], [CreatedAt])
                VALUES
                ('66666666-6666-6666-6666-666666666661', '33333333-3333-3333-3333-333333333331', N'NovaStay Riverside', N'12 Nguyen Huu Canh, Binh Thanh, Ho Chi Minh City', 'Apartment', '2026-06-20T09:20:00');

                INSERT INTO [StaffUserPropertyMapping] ([StaffUserId], [PropertyId])
                VALUES
                ('55555555-5555-5555-5555-555555555551', '66666666-6666-6666-6666-666666666661');

                INSERT INTO [Rooms] ([Id], [PropertyId], [RoomNumber], [Floor], [BasePrice], [Status], [MaxOccupants], [CreatedAt])
                VALUES
                ('77777777-7777-7777-7777-777777777771', '66666666-6666-6666-6666-666666666661', 'A101', 1, 4500000.00, 'Available', 2, '2026-06-20T09:25:00'),
                ('77777777-7777-7777-7777-777777777772', '66666666-6666-6666-6666-666666666661', 'A102', 1, 5200000.00, 'Occupied', 2, '2026-06-20T09:25:00');

                INSERT INTO [RoomImages] ([Id], [RoomId], [ImageUrl], [IsCover], [UploadedAt])
                VALUES
                ('88888888-8888-8888-8888-888888888881', '77777777-7777-7777-7777-777777777771', 'https://example.local/images/rooms/a101-cover.jpg', 1, '2026-06-20T09:30:00');

                INSERT INTO [RoomAvailabilities] ([Id], [RoomId], [StayDate], [DynamicPrice], [IsBooked])
                VALUES
                ('99999999-9999-9999-9999-999999999991', '77777777-7777-7777-7777-777777777771', '2026-07-01', 4500000.00, 0),
                ('99999999-9999-9999-9999-999999999992', '77777777-7777-7777-7777-777777777772', '2026-07-01', 5200000.00, 1);

                INSERT INTO [Assets] ([Id], [OrganizationId], [AssetName], [Brand], [Model], [AssetCode], [PurchaseDate], [WarrantyExpiryDate], [BaseValue], [CreatedAt])
                VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '33333333-3333-3333-3333-333333333331', N'Air Conditioner', N'Daikin', 'FTKQ25', 'AC-A101-001', '2026-01-10', '2028-01-10', 8500000.00, '2026-06-20T09:35:00');

                INSERT INTO [AssetAssignments] ([Id], [OrganizationId], [AssetId], [RoomId], [Status], [Note], [AssignedAt])
                VALUES
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', '33333333-3333-3333-3333-333333333331', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '77777777-7777-7777-7777-777777777771', 'Good', N'Installed in bedroom area.', '2026-06-20T09:40:00');

                INSERT INTO [Residents] ([Id], [OrganizationId], [FullName], [Phone], [Email], [IdentityCardNumber], [IdFrontImageUrl], [IdBackImageUrl], [ProfileImageUrl], [CreatedAt])
                VALUES
                ('cccccccc-cccc-cccc-cccc-ccccccccccc1', '33333333-3333-3333-3333-333333333331', N'Nguyen Van Demo', '0901000003', 'resident.demo@novastay.local', '079200000001', 'https://example.local/images/residents/id-front.jpg', 'https://example.local/images/residents/id-back.jpg', 'https://example.local/images/residents/profile.jpg', '2026-06-20T09:45:00');

                INSERT INTO [Brokers] ([Id], [OrganizationId], [FullName], [Phone], [WalletBalance], [TotalCommissionEarned])
                VALUES
                ('dddddddd-dddd-dddd-dddd-ddddddddddd1', '33333333-3333-3333-3333-333333333331', N'Tran Thi Broker', '0901000004', 1500000.00, 3500000.00);

                INSERT INTO [Bookings] ([Id], [OrganizationId], [PropertyId], [RoomId], [GuestName], [GuestPhone], [GuestEmail], [BookingType], [CheckInDate], [CheckOutDate], [Amount], [PaymentStatus], [PaymentTransactionId], [CreatedAt])
                VALUES
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1', '33333333-3333-3333-3333-333333333331', '66666666-6666-6666-6666-666666666661', '77777777-7777-7777-7777-777777777772', N'Nguyen Van Demo', '0901000003', 'resident.demo@novastay.local', 'LongTerm', '2026-07-01', '2027-06-30', 5200000.00, 'Paid', 'TXN-DEMO-0001', '2026-06-20T09:50:00');

                INSERT INTO [Contracts] ([Id], [OrganizationId], [PropertyId], [RoomId], [ResidentId], [BrokerId], [BookingId], [StartDate], [EndDate], [DepositAmount], [BrokerCommission], [CommissionStatus], [ContractPdfUrl], [Status], [CreatedAt])
                VALUES
                ('ffffffff-ffff-ffff-ffff-fffffffffff1', '33333333-3333-3333-3333-333333333331', '66666666-6666-6666-6666-666666666661', '77777777-7777-7777-7777-777777777772', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'dddddddd-dddd-dddd-dddd-ddddddddddd1', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1', '2026-07-01', '2027-06-30', 10400000.00, 2600000.00, 'Pending', 'https://example.local/contracts/demo-contract.pdf', 'Active', '2026-06-20T09:55:00');

                INSERT INTO [Invoices] ([Id], [OrganizationId], [ContractId], [BookingId], [InvoicePeriod], [RoomPrice], [ServicesPrice], [TotalAmount], [QrCodeUrl], [Status], [PaidAt], [CreatedAt])
                VALUES
                ('12121212-1212-1212-1212-121212121211', '33333333-3333-3333-3333-333333333331', 'ffffffff-ffff-ffff-ffff-fffffffffff1', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1', '2026-07', 5200000.00, 350000.00, 5550000.00, 'https://example.local/invoices/qr-2026-07.png', 'Paid', '2026-06-20T10:00:00', '2026-06-20T10:00:00');

                INSERT INTO [Technicians] ([Id], [OrganizationId], [FullName], [Phone], [Specialty], [IsAvailable])
                VALUES
                ('13131313-1313-1313-1313-131313131311', '33333333-3333-3333-3333-333333333331', N'Le Van Technician', '0901000005', N'Electrical', 1);

                INSERT INTO [MaintenanceTickets] ([Id], [OrganizationId], [RoomId], [ResidentId], [AssetAssignmentId], [Category], [UserDescription], [IncidentImageUrl], [Status], [TechnicianId], [ResolvedImageUrl], [CreatedAt], [UpdatedAt])
                VALUES
                ('14141414-1414-1414-1414-141414141411', '33333333-3333-3333-3333-333333333331', '77777777-7777-7777-7777-777777777771', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', N'Air conditioner', N'Air conditioner makes noise during the night.', 'https://example.local/tickets/ac-noise-before.jpg', 'InProgress', '13131313-1313-1313-1313-131313131311', NULL, '2026-06-20T10:05:00', '2026-06-20T10:10:00');

                INSERT INTO [AccountAccessTokens] ([Id], [StaffUserId], [TokenHash], [ExpiresAt], [RevokedAt], [CreatedAt], [CreatedByIp], [RevokedByIp])
                VALUES
                ('15151515-1515-1515-1515-151515151511', '55555555-5555-5555-5555-555555555551', 'sample-access-token-hash-demo', '2026-06-20T12:00:00', NULL, '2026-06-20T10:15:00', '127.0.0.1', NULL);

                INSERT INTO [AccountRefreshTokens] ([Id], [StaffUserId], [TokenHash], [ExpiresAt], [RevokedAt], [CreatedAt], [CreatedByIp], [RevokedByIp], [ReplacedByTokenHash])
                VALUES
                ('16161616-1616-1616-1616-161616161611', '55555555-5555-5555-5555-555555555551', 'sample-refresh-token-hash-demo', '2026-07-20T10:15:00', NULL, '2026-06-20T10:15:00', '127.0.0.1', NULL, NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [AccountRefreshTokens] WHERE [Id] = '16161616-1616-1616-1616-161616161611';
                DELETE FROM [AccountAccessTokens] WHERE [Id] = '15151515-1515-1515-1515-151515151511';
                DELETE FROM [MaintenanceTickets] WHERE [Id] = '14141414-1414-1414-1414-141414141411';
                DELETE FROM [Invoices] WHERE [Id] = '12121212-1212-1212-1212-121212121211';
                DELETE FROM [Contracts] WHERE [Id] = 'ffffffff-ffff-ffff-ffff-fffffffffff1';
                DELETE FROM [Bookings] WHERE [Id] = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1';
                DELETE FROM [Brokers] WHERE [Id] = 'dddddddd-dddd-dddd-dddd-ddddddddddd1';
                DELETE FROM [Technicians] WHERE [Id] = '13131313-1313-1313-1313-131313131311';
                DELETE FROM [Residents] WHERE [Id] = 'cccccccc-cccc-cccc-cccc-ccccccccccc1';
                DELETE FROM [AssetAssignments] WHERE [Id] = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1';
                DELETE FROM [Assets] WHERE [Id] = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1';
                DELETE FROM [RoomAvailabilities] WHERE [Id] IN ('99999999-9999-9999-9999-999999999991', '99999999-9999-9999-9999-999999999992');
                DELETE FROM [RoomImages] WHERE [Id] = '88888888-8888-8888-8888-888888888881';
                DELETE FROM [StaffUserPropertyMapping] WHERE [StaffUserId] = '55555555-5555-5555-5555-555555555551' AND [PropertyId] = '66666666-6666-6666-6666-666666666661';
                DELETE FROM [Rooms] WHERE [Id] IN ('77777777-7777-7777-7777-777777777771', '77777777-7777-7777-7777-777777777772');
                DELETE FROM [Properties] WHERE [Id] = '66666666-6666-6666-6666-666666666661';
                DELETE FROM [StaffUserRoles] WHERE [StaffUserId] = '55555555-5555-5555-5555-555555555551' AND [RoleId] = '44444444-4444-4444-4444-444444444441';
                DELETE FROM [StaffUsers] WHERE [Id] = '55555555-5555-5555-5555-555555555551';
                DELETE FROM [RolePermissions] WHERE [RoleId] = '44444444-4444-4444-4444-444444444441';
                DELETE FROM [Roles] WHERE [Id] = '44444444-4444-4444-4444-444444444441';
                DELETE FROM [Organizations] WHERE [Id] = '33333333-3333-3333-3333-333333333331';
                DELETE FROM [Permissions] WHERE [Id] IN ('22222222-2222-2222-2222-222222222221', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222223', '22222222-2222-2222-2222-222222222224');
                DELETE FROM [SubscriptionPackages] WHERE [Id] IN ('11111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111112');
                """);
        }
    }
}
