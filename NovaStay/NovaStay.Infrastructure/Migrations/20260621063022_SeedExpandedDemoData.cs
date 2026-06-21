using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedExpandedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [SubscriptionPackages] ([Id], [PackageName], [PackageKey], [PriceMonthly], [MaxProperties], [MaxRooms], [TokenGiftMonthly], [AllowCustomRoles], [AllowAiFeatures], [CreatedAt])
                VALUES
                ('11111111-1111-1111-1111-111111111113', N'NovaStay Business', 'BUSINESS', 1299000.00, 35, 500, 1200, 1, 1, '2026-06-21T09:00:00'),
                ('11111111-1111-1111-1111-111111111114', N'NovaStay Enterprise', 'ENTERPRISE', 2499000.00, 100, 2000, 3000, 1, 1, '2026-06-21T09:00:00'),
                ('11111111-1111-1111-1111-111111111115', N'NovaStay Trial', 'TRIAL', 0.00, 1, 10, 25, 0, 0, '2026-06-21T09:00:00');

                INSERT INTO [Permissions] ([Id], [PermissionName], [PermissionKey], [Module])
                VALUES
                ('22222222-2222-2222-2222-222222222225', N'View residents', 'residents.view', 'residents'),
                ('22222222-2222-2222-2222-222222222226', N'Create maintenance tickets', 'maintenance.create', 'maintenance'),
                ('22222222-2222-2222-2222-222222222227', N'View invoices', 'invoices.view', 'invoices'),
                ('22222222-2222-2222-2222-222222222228', N'Manage own profile', 'profile.manage', 'profile');

                UPDATE [Roles]
                SET [RoleName] = N'Staff',
                    [RoleKey] = 'STAFF',
                    [Description] = N'Staff can operate assigned properties, rooms, contracts, invoices, and tickets.'
                WHERE [Id] = '44444444-4444-4444-4444-444444444441';

                INSERT INTO [Roles] ([Id], [OrganizationId], [RoleName], [RoleKey], [Description], [CreatedAt])
                VALUES
                ('44444444-4444-4444-4444-444444444442', '33333333-3333-3333-3333-333333333331', N'Owner', 'OWNER', N'Business owner with full organization access.', '2026-06-21T09:05:00'),
                ('44444444-4444-4444-4444-444444444443', '33333333-3333-3333-3333-333333333331', N'Resident', 'RESIDENT', N'Resident can view their room, contract, invoices, and maintenance tickets.', '2026-06-21T09:05:00');

                INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
                VALUES
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222221'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222222'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222223'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222224'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222225'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222226'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222227'),
                ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222228'),
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222225'),
                ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222226'),
                ('44444444-4444-4444-4444-444444444443', '22222222-2222-2222-2222-222222222226'),
                ('44444444-4444-4444-4444-444444444443', '22222222-2222-2222-2222-222222222227'),
                ('44444444-4444-4444-4444-444444444443', '22222222-2222-2222-2222-222222222228');

                INSERT INTO [Accounts] ([Id], [AccountType], [Email], [Phone], [PasswordHash], [IsActive], [CreatedAt])
                VALUES
                ('a0000000-0000-0000-0000-000000000001', 'BusinessOwner', 'owner.green@novastay.local', '0911000001', 'sample-owner-hash-001', 1, '2026-06-21T09:10:00'),
                ('a0000000-0000-0000-0000-000000000002', 'BusinessOwner', 'owner.sky@novastay.local', '0911000002', 'sample-owner-hash-002', 1, '2026-06-21T09:10:00'),
                ('a0000000-0000-0000-0000-000000000003', 'BusinessOwner', 'owner.lotus@novastay.local', '0911000003', 'sample-owner-hash-003', 1, '2026-06-21T09:10:00'),
                ('a0000000-0000-0000-0000-000000000004', 'BusinessOwner', 'owner.sun@novastay.local', '0911000004', 'sample-owner-hash-004', 1, '2026-06-21T09:10:00'),
                ('a0000000-0000-0000-0000-000000000011', 'Staff', 'staff.green@novastay.local', '0911000011', 'sample-staff-hash-001', 1, '2026-06-21T09:12:00'),
                ('a0000000-0000-0000-0000-000000000012', 'Staff', 'staff.sky@novastay.local', '0911000012', 'sample-staff-hash-002', 1, '2026-06-21T09:12:00'),
                ('a0000000-0000-0000-0000-000000000013', 'Staff', 'staff.lotus@novastay.local', '0911000013', 'sample-staff-hash-003', 1, '2026-06-21T09:12:00'),
                ('a0000000-0000-0000-0000-000000000014', 'Staff', 'staff.sun@novastay.local', '0911000014', 'sample-staff-hash-004', 1, '2026-06-21T09:12:00'),
                ('a0000000-0000-0000-0000-000000000021', 'Resident', 'resident.anh@novastay.local', '0911000021', 'sample-resident-hash-001', 1, '2026-06-21T09:14:00'),
                ('a0000000-0000-0000-0000-000000000022', 'Resident', 'resident.binh@novastay.local', '0911000022', 'sample-resident-hash-002', 1, '2026-06-21T09:14:00'),
                ('a0000000-0000-0000-0000-000000000023', 'Resident', 'resident.chi@novastay.local', '0911000023', 'sample-resident-hash-003', 1, '2026-06-21T09:14:00'),
                ('a0000000-0000-0000-0000-000000000024', 'Resident', 'resident.dung@novastay.local', '0911000024', 'sample-resident-hash-004', 1, '2026-06-21T09:14:00'),
                ('a0000000-0000-0000-0000-000000000025', 'Resident', 'resident.em@novastay.local', '0911000025', 'sample-resident-hash-005', 1, '2026-06-21T09:14:00');

                INSERT INTO [Organizations] ([Id], [OwnerAccountId], [PackageId], [BusinessName], [TaxCode], [OwnerEmail], [OwnerPhone], [SubscriptionStatus], [TokenBalance], [CreatedAt], [UpdatedAt])
                VALUES
                ('33333333-3333-3333-3333-333333333332', 'a0000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111113', N'Green House Co', '0312345601', 'owner.green@novastay.local', '0911000001', 'Active', 900, '2026-06-21T09:20:00', '2026-06-21T09:20:00'),
                ('33333333-3333-3333-3333-333333333333', 'a0000000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111112', N'Skyline Stay JSC', '0312345602', 'owner.sky@novastay.local', '0911000002', 'Active', 650, '2026-06-21T09:20:00', '2026-06-21T09:20:00'),
                ('33333333-3333-3333-3333-333333333334', 'a0000000-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111114', N'Lotus Home Services', '0312345603', 'owner.lotus@novastay.local', '0911000003', 'Active', 1400, '2026-06-21T09:20:00', '2026-06-21T09:20:00'),
                ('33333333-3333-3333-3333-333333333335', 'a0000000-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111115', N'Sunrise Mini Apartment', '0312345604', 'owner.sun@novastay.local', '0911000004', 'Trial', 120, '2026-06-21T09:20:00', '2026-06-21T09:20:00');

                INSERT INTO [StaffUsers] ([Id], [AccountId], [OrganizationId], [FullName], [IsActive], [CreatedAt])
                VALUES
                ('55555555-5555-5555-5555-555555555552', 'a0000000-0000-0000-0000-000000000011', '33333333-3333-3333-3333-333333333332', N'Pham Thi Green Staff', 1, '2026-06-21T09:25:00'),
                ('55555555-5555-5555-5555-555555555553', 'a0000000-0000-0000-0000-000000000012', '33333333-3333-3333-3333-333333333333', N'Le Van Sky Staff', 1, '2026-06-21T09:25:00'),
                ('55555555-5555-5555-5555-555555555554', 'a0000000-0000-0000-0000-000000000013', '33333333-3333-3333-3333-333333333334', N'Tran Lotus Staff', 1, '2026-06-21T09:25:00'),
                ('55555555-5555-5555-5555-555555555555', 'a0000000-0000-0000-0000-000000000014', '33333333-3333-3333-3333-333333333335', N'Nguyen Sunrise Staff', 1, '2026-06-21T09:25:00');

                INSERT INTO [Properties] ([Id], [OrganizationId], [PropertyName], [Address], [PropertyType], [CreatedAt])
                VALUES
                ('66666666-6666-6666-6666-666666666662', '33333333-3333-3333-3333-333333333332', N'Green House District 7', N'45 Nguyen Thi Thap, District 7, Ho Chi Minh City', 'Apartment', '2026-06-21T09:30:00'),
                ('66666666-6666-6666-6666-666666666663', '33333333-3333-3333-3333-333333333333', N'Skyline Studio Tan Binh', N'18 Cong Hoa, Tan Binh, Ho Chi Minh City', 'Studio', '2026-06-21T09:30:00'),
                ('66666666-6666-6666-6666-666666666664', '33333333-3333-3333-3333-333333333334', N'Lotus Home Thu Duc', N'88 Vo Van Ngan, Thu Duc, Ho Chi Minh City', 'House', '2026-06-21T09:30:00'),
                ('66666666-6666-6666-6666-666666666665', '33333333-3333-3333-3333-333333333335', N'Sunrise Mini Binh Thanh', N'22 Xo Viet Nghe Tinh, Binh Thanh, Ho Chi Minh City', 'MiniApartment', '2026-06-21T09:30:00');

                INSERT INTO [StaffUserPropertyMapping] ([StaffUserId], [PropertyId])
                VALUES
                ('55555555-5555-5555-5555-555555555552', '66666666-6666-6666-6666-666666666662'),
                ('55555555-5555-5555-5555-555555555553', '66666666-6666-6666-6666-666666666663'),
                ('55555555-5555-5555-5555-555555555554', '66666666-6666-6666-6666-666666666664'),
                ('55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666665');

                INSERT INTO [Rooms] ([Id], [PropertyId], [RoomNumber], [Floor], [BasePrice], [Status], [MaxOccupants], [CreatedAt])
                VALUES
                ('77777777-7777-7777-7777-777777777773', '66666666-6666-6666-6666-666666666662', 'G201', 2, 4800000.00, 'Available', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777774', '66666666-6666-6666-6666-666666666662', 'G202', 2, 5100000.00, 'Occupied', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777775', '66666666-6666-6666-6666-666666666663', 'S301', 3, 6200000.00, 'Occupied', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777776', '66666666-6666-6666-6666-666666666663', 'S302', 3, 6400000.00, 'Available', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777777', '66666666-6666-6666-6666-666666666664', 'L101', 1, 4300000.00, 'Occupied', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777778', '66666666-6666-6666-6666-666666666664', 'L102', 1, 4500000.00, 'Available', 2, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-777777777779', '66666666-6666-6666-6666-666666666665', 'M401', 4, 3900000.00, 'Occupied', 1, '2026-06-21T09:35:00'),
                ('77777777-7777-7777-7777-77777777777a', '66666666-6666-6666-6666-666666666665', 'M402', 4, 4100000.00, 'Available', 1, '2026-06-21T09:35:00');

                INSERT INTO [RoomImages] ([Id], [RoomId], [ImageUrl], [IsCover], [UploadedAt])
                VALUES
                ('88888888-8888-8888-8888-888888888882', '77777777-7777-7777-7777-777777777773', 'https://example.local/images/rooms/g201-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888883', '77777777-7777-7777-7777-777777777774', 'https://example.local/images/rooms/g202-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888884', '77777777-7777-7777-7777-777777777775', 'https://example.local/images/rooms/s301-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888885', '77777777-7777-7777-7777-777777777776', 'https://example.local/images/rooms/s302-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888886', '77777777-7777-7777-7777-777777777777', 'https://example.local/images/rooms/l101-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888887', '77777777-7777-7777-7777-777777777778', 'https://example.local/images/rooms/l102-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888888', '77777777-7777-7777-7777-777777777779', 'https://example.local/images/rooms/m401-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-888888888889', '77777777-7777-7777-7777-77777777777a', 'https://example.local/images/rooms/m402-cover.jpg', 1, '2026-06-21T09:40:00'),
                ('88888888-8888-8888-8888-88888888888a', '77777777-7777-7777-7777-777777777773', 'https://example.local/images/rooms/g201-bathroom.jpg', 0, '2026-06-21T09:40:00');

                INSERT INTO [RoomAvailabilities] ([Id], [RoomId], [StayDate], [DynamicPrice], [IsBooked])
                VALUES
                ('99999999-9999-9999-9999-999999999993', '77777777-7777-7777-7777-777777777773', '2026-07-01', 4800000.00, 0),
                ('99999999-9999-9999-9999-999999999994', '77777777-7777-7777-7777-777777777774', '2026-07-01', 5100000.00, 1),
                ('99999999-9999-9999-9999-999999999995', '77777777-7777-7777-7777-777777777775', '2026-07-01', 6200000.00, 1),
                ('99999999-9999-9999-9999-999999999996', '77777777-7777-7777-7777-777777777776', '2026-07-01', 6400000.00, 0),
                ('99999999-9999-9999-9999-999999999997', '77777777-7777-7777-7777-777777777777', '2026-07-01', 4300000.00, 1),
                ('99999999-9999-9999-9999-999999999998', '77777777-7777-7777-7777-777777777778', '2026-07-01', 4500000.00, 0),
                ('99999999-9999-9999-9999-999999999999', '77777777-7777-7777-7777-777777777779', '2026-07-01', 3900000.00, 1),
                ('99999999-9999-9999-9999-99999999999a', '77777777-7777-7777-7777-77777777777a', '2026-07-01', 4100000.00, 0);

                INSERT INTO [Assets] ([Id], [OrganizationId], [AssetName], [Brand], [Model], [AssetCode], [PurchaseDate], [WarrantyExpiryDate], [BaseValue], [CreatedAt])
                VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333332', N'Washing Machine', N'LG', 'T2312', 'WM-G202-001', '2026-02-01', '2028-02-01', 7200000.00, '2026-06-21T09:45:00'),
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '33333333-3333-3333-3333-333333333333', N'Refrigerator', N'Panasonic', 'NR-BX', 'RF-S301-001', '2026-02-05', '2028-02-05', 9300000.00, '2026-06-21T09:45:00'),
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', '33333333-3333-3333-3333-333333333334', N'Bed Frame', N'IKEA', 'MALM', 'BED-L101-001', '2026-03-10', '2028-03-10', 3500000.00, '2026-06-21T09:45:00'),
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', '33333333-3333-3333-3333-333333333335', N'Water Heater', N'Ariston', 'AN2', 'WH-M401-001', '2026-04-12', '2028-04-12', 4100000.00, '2026-06-21T09:45:00');

                INSERT INTO [AssetAssignments] ([Id], [OrganizationId], [AssetId], [RoomId], [Status], [Note], [AssignedAt])
                VALUES
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', '33333333-3333-3333-3333-333333333332', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '77777777-7777-7777-7777-777777777774', 'Good', N'Placed in laundry corner.', '2026-06-21T09:50:00'),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', '33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '77777777-7777-7777-7777-777777777775', 'Good', N'Kitchen refrigerator.', '2026-06-21T09:50:00'),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4', '33333333-3333-3333-3333-333333333334', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', '77777777-7777-7777-7777-777777777777', 'Good', N'Main bedroom bed frame.', '2026-06-21T09:50:00'),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5', '33333333-3333-3333-3333-333333333335', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', '77777777-7777-7777-7777-777777777779', 'Maintenance', N'Needs inspection next month.', '2026-06-21T09:50:00');

                INSERT INTO [Residents] ([Id], [AccountId], [OrganizationId], [FullName], [Phone], [Email], [IdentityCardNumber], [IdFrontImageUrl], [IdBackImageUrl], [ProfileImageUrl], [CreatedAt])
                VALUES
                ('cccccccc-cccc-cccc-cccc-ccccccccccc2', 'a0000000-0000-0000-0000-000000000021', '33333333-3333-3333-3333-333333333332', N'Hoang Minh Anh', '0911000021', 'resident.anh@novastay.local', '079200000002', 'https://example.local/images/residents/anh-front.jpg', 'https://example.local/images/residents/anh-back.jpg', 'https://example.local/images/residents/anh-profile.jpg', '2026-06-21T09:55:00'),
                ('cccccccc-cccc-cccc-cccc-ccccccccccc3', 'a0000000-0000-0000-0000-000000000022', '33333333-3333-3333-3333-333333333333', N'Nguyen Quang Binh', '0911000022', 'resident.binh@novastay.local', '079200000003', 'https://example.local/images/residents/binh-front.jpg', 'https://example.local/images/residents/binh-back.jpg', 'https://example.local/images/residents/binh-profile.jpg', '2026-06-21T09:55:00'),
                ('cccccccc-cccc-cccc-cccc-ccccccccccc4', 'a0000000-0000-0000-0000-000000000023', '33333333-3333-3333-3333-333333333334', N'Tran Ngoc Chi', '0911000023', 'resident.chi@novastay.local', '079200000004', 'https://example.local/images/residents/chi-front.jpg', 'https://example.local/images/residents/chi-back.jpg', 'https://example.local/images/residents/chi-profile.jpg', '2026-06-21T09:55:00'),
                ('cccccccc-cccc-cccc-cccc-ccccccccccc5', 'a0000000-0000-0000-0000-000000000024', '33333333-3333-3333-3333-333333333335', N'Pham Thanh Dung', '0911000024', 'resident.dung@novastay.local', '079200000005', 'https://example.local/images/residents/dung-front.jpg', 'https://example.local/images/residents/dung-back.jpg', 'https://example.local/images/residents/dung-profile.jpg', '2026-06-21T09:55:00'),
                ('cccccccc-cccc-cccc-cccc-ccccccccccc6', 'a0000000-0000-0000-0000-000000000025', '33333333-3333-3333-3333-333333333332', N'Le Bao Em', '0911000025', 'resident.em@novastay.local', '079200000006', 'https://example.local/images/residents/em-front.jpg', 'https://example.local/images/residents/em-back.jpg', 'https://example.local/images/residents/em-profile.jpg', '2026-06-21T09:55:00');

                INSERT INTO [Brokers] ([Id], [OrganizationId], [FullName], [Phone], [WalletBalance], [TotalCommissionEarned])
                VALUES
                ('dddddddd-dddd-dddd-dddd-ddddddddddd2', '33333333-3333-3333-3333-333333333332', N'Vo Broker Green', '0911000031', 800000.00, 1800000.00),
                ('dddddddd-dddd-dddd-dddd-ddddddddddd3', '33333333-3333-3333-3333-333333333333', N'Dang Broker Sky', '0911000032', 1200000.00, 2200000.00),
                ('dddddddd-dddd-dddd-dddd-ddddddddddd4', '33333333-3333-3333-3333-333333333334', N'Bui Broker Lotus', '0911000033', 600000.00, 1600000.00),
                ('dddddddd-dddd-dddd-dddd-ddddddddddd5', '33333333-3333-3333-3333-333333333335', N'Mai Broker Sunrise', '0911000034', 300000.00, 900000.00);

                INSERT INTO [Bookings] ([Id], [OrganizationId], [PropertyId], [RoomId], [GuestName], [GuestPhone], [GuestEmail], [BookingType], [CheckInDate], [CheckOutDate], [Amount], [PaymentStatus], [PaymentTransactionId], [CreatedAt])
                VALUES
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', '33333333-3333-3333-3333-333333333332', '66666666-6666-6666-6666-666666666662', '77777777-7777-7777-7777-777777777774', N'Hoang Minh Anh', '0911000021', 'resident.anh@novastay.local', 'LongTerm', '2026-07-05', '2027-07-04', 5100000.00, 'Paid', 'TXN-DEMO-0002', '2026-06-21T10:00:00'),
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3', '33333333-3333-3333-3333-333333333333', '66666666-6666-6666-6666-666666666663', '77777777-7777-7777-7777-777777777775', N'Nguyen Quang Binh', '0911000022', 'resident.binh@novastay.local', 'LongTerm', '2026-07-10', '2027-07-09', 6200000.00, 'Paid', 'TXN-DEMO-0003', '2026-06-21T10:00:00'),
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4', '33333333-3333-3333-3333-333333333334', '66666666-6666-6666-6666-666666666664', '77777777-7777-7777-7777-777777777777', N'Tran Ngoc Chi', '0911000023', 'resident.chi@novastay.local', 'LongTerm', '2026-07-15', '2027-07-14', 4300000.00, 'Pending', 'TXN-DEMO-0004', '2026-06-21T10:00:00'),
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5', '33333333-3333-3333-3333-333333333335', '66666666-6666-6666-6666-666666666665', '77777777-7777-7777-7777-777777777779', N'Pham Thanh Dung', '0911000024', 'resident.dung@novastay.local', 'ShortTerm', '2026-07-20', '2026-12-19', 3900000.00, 'Paid', 'TXN-DEMO-0005', '2026-06-21T10:00:00');

                INSERT INTO [Contracts] ([Id], [OrganizationId], [PropertyId], [RoomId], [ResidentId], [BrokerId], [BookingId], [StartDate], [EndDate], [DepositAmount], [BrokerCommission], [CommissionStatus], [ContractPdfUrl], [Status], [CreatedAt])
                VALUES
                ('ffffffff-ffff-ffff-ffff-fffffffffff2', '33333333-3333-3333-3333-333333333332', '66666666-6666-6666-6666-666666666662', '77777777-7777-7777-7777-777777777774', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 'dddddddd-dddd-dddd-dddd-ddddddddddd2', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', '2026-07-05', '2027-07-04', 10200000.00, 2550000.00, 'Pending', 'https://example.local/contracts/green-anh.pdf', 'Active', '2026-06-21T10:05:00'),
                ('ffffffff-ffff-ffff-ffff-fffffffffff3', '33333333-3333-3333-3333-333333333333', '66666666-6666-6666-6666-666666666663', '77777777-7777-7777-7777-777777777775', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 'dddddddd-dddd-dddd-dddd-ddddddddddd3', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3', '2026-07-10', '2027-07-09', 12400000.00, 3100000.00, 'Paid', 'https://example.local/contracts/sky-binh.pdf', 'Active', '2026-06-21T10:05:00'),
                ('ffffffff-ffff-ffff-ffff-fffffffffff4', '33333333-3333-3333-3333-333333333334', '66666666-6666-6666-6666-666666666664', '77777777-7777-7777-7777-777777777777', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 'dddddddd-dddd-dddd-dddd-ddddddddddd4', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4', '2026-07-15', '2027-07-14', 8600000.00, 2150000.00, 'Pending', 'https://example.local/contracts/lotus-chi.pdf', 'Active', '2026-06-21T10:05:00'),
                ('ffffffff-ffff-ffff-ffff-fffffffffff5', '33333333-3333-3333-3333-333333333335', '66666666-6666-6666-6666-666666666665', '77777777-7777-7777-7777-777777777779', 'cccccccc-cccc-cccc-cccc-ccccccccccc5', 'dddddddd-dddd-dddd-dddd-ddddddddddd5', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5', '2026-07-20', '2026-12-19', 7800000.00, 1950000.00, 'Pending', 'https://example.local/contracts/sun-dung.pdf', 'Active', '2026-06-21T10:05:00');

                INSERT INTO [Invoices] ([Id], [OrganizationId], [ContractId], [BookingId], [InvoicePeriod], [RoomPrice], [ServicesPrice], [TotalAmount], [QrCodeUrl], [Status], [PaidAt], [CreatedAt])
                VALUES
                ('12121212-1212-1212-1212-121212121212', '33333333-3333-3333-3333-333333333332', 'ffffffff-ffff-ffff-ffff-fffffffffff2', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', '2026-07', 5100000.00, 400000.00, 5500000.00, 'https://example.local/invoices/qr-2026-07-green.png', 'Paid', '2026-06-21T10:10:00', '2026-06-21T10:10:00'),
                ('12121212-1212-1212-1212-121212121213', '33333333-3333-3333-3333-333333333333', 'ffffffff-ffff-ffff-ffff-fffffffffff3', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3', '2026-07', 6200000.00, 500000.00, 6700000.00, 'https://example.local/invoices/qr-2026-07-sky.png', 'Paid', '2026-06-21T10:10:00', '2026-06-21T10:10:00'),
                ('12121212-1212-1212-1212-121212121214', '33333333-3333-3333-3333-333333333334', 'ffffffff-ffff-ffff-ffff-fffffffffff4', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4', '2026-07', 4300000.00, 350000.00, 4650000.00, 'https://example.local/invoices/qr-2026-07-lotus.png', 'Unpaid', NULL, '2026-06-21T10:10:00'),
                ('12121212-1212-1212-1212-121212121215', '33333333-3333-3333-3333-333333333335', 'ffffffff-ffff-ffff-ffff-fffffffffff5', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5', '2026-07', 3900000.00, 300000.00, 4200000.00, 'https://example.local/invoices/qr-2026-07-sun.png', 'Paid', '2026-06-21T10:10:00', '2026-06-21T10:10:00');

                INSERT INTO [Technicians] ([Id], [OrganizationId], [FullName], [Phone], [Specialty], [IsAvailable])
                VALUES
                ('13131313-1313-1313-1313-131313131312', '33333333-3333-3333-3333-333333333332', N'Green Electrical Team', '0911000041', N'Electrical', 1),
                ('13131313-1313-1313-1313-131313131313', '33333333-3333-3333-3333-333333333333', N'Sky Plumbing Team', '0911000042', N'Plumbing', 1),
                ('13131313-1313-1313-1313-131313131314', '33333333-3333-3333-3333-333333333334', N'Lotus AC Team', '0911000043', N'Air conditioner', 0),
                ('13131313-1313-1313-1313-131313131315', '33333333-3333-3333-3333-333333333335', N'Sunrise General Team', '0911000044', N'General', 1);

                INSERT INTO [MaintenanceTickets] ([Id], [OrganizationId], [RoomId], [ResidentId], [AssetAssignmentId], [Category], [UserDescription], [IncidentImageUrl], [Status], [TechnicianId], [ResolvedImageUrl], [CreatedAt], [UpdatedAt])
                VALUES
                ('14141414-1414-1414-1414-141414141412', '33333333-3333-3333-3333-333333333332', '77777777-7777-7777-7777-777777777774', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', N'Washing machine', N'Washing machine vibrates heavily.', 'https://example.local/tickets/wm-before.jpg', 'Pending', '13131313-1313-1313-1313-131313131312', NULL, '2026-06-21T10:15:00', '2026-06-21T10:15:00'),
                ('14141414-1414-1414-1414-141414141413', '33333333-3333-3333-3333-333333333333', '77777777-7777-7777-7777-777777777775', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', N'Refrigerator', N'Refrigerator cooling is weak.', 'https://example.local/tickets/fridge-before.jpg', 'InProgress', '13131313-1313-1313-1313-131313131313', NULL, '2026-06-21T10:15:00', '2026-06-21T10:20:00'),
                ('14141414-1414-1414-1414-141414141414', '33333333-3333-3333-3333-333333333334', '77777777-7777-7777-7777-777777777777', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4', N'Furniture', N'Bed frame screw is loose.', 'https://example.local/tickets/bed-before.jpg', 'Resolved', '13131313-1313-1313-1313-131313131314', 'https://example.local/tickets/bed-after.jpg', '2026-06-21T10:15:00', '2026-06-21T11:00:00'),
                ('14141414-1414-1414-1414-141414141415', '33333333-3333-3333-3333-333333333335', '77777777-7777-7777-7777-777777777779', 'cccccccc-cccc-cccc-cccc-ccccccccccc5', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5', N'Water heater', N'Water heater temperature is unstable.', 'https://example.local/tickets/heater-before.jpg', 'Pending', '13131313-1313-1313-1313-131313131315', NULL, '2026-06-21T10:15:00', '2026-06-21T10:15:00');

                INSERT INTO [AccountAccessTokens] ([Id], [AccountId], [TokenHash], [ExpiresAt], [RevokedAt], [CreatedAt], [CreatedByIp], [RevokedByIp])
                VALUES
                ('15151515-1515-1515-1515-151515151512', 'a0000000-0000-0000-0000-000000000001', 'sample-access-token-owner-green', '2026-07-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL),
                ('15151515-1515-1515-1515-151515151513', 'a0000000-0000-0000-0000-000000000011', 'sample-access-token-staff-green', '2026-07-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL),
                ('15151515-1515-1515-1515-151515151514', 'a0000000-0000-0000-0000-000000000021', 'sample-access-token-resident-anh', '2026-07-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL),
                ('15151515-1515-1515-1515-151515151515', 'a0000000-0000-0000-0000-000000000022', 'sample-access-token-resident-binh', '2026-07-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL),
                ('15151515-1515-1515-1515-151515151516', 'a0000000-0000-0000-0000-000000000012', 'sample-access-token-staff-sky', '2026-07-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL);

                INSERT INTO [AccountRefreshTokens] ([Id], [AccountId], [TokenHash], [ExpiresAt], [RevokedAt], [CreatedAt], [CreatedByIp], [RevokedByIp], [ReplacedByTokenHash])
                VALUES
                ('16161616-1616-1616-1616-161616161612', 'a0000000-0000-0000-0000-000000000001', 'sample-refresh-token-owner-green', '2026-08-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL, NULL),
                ('16161616-1616-1616-1616-161616161613', 'a0000000-0000-0000-0000-000000000011', 'sample-refresh-token-staff-green', '2026-08-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL, NULL),
                ('16161616-1616-1616-1616-161616161614', 'a0000000-0000-0000-0000-000000000021', 'sample-refresh-token-resident-anh', '2026-08-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL, NULL),
                ('16161616-1616-1616-1616-161616161615', 'a0000000-0000-0000-0000-000000000022', 'sample-refresh-token-resident-binh', '2026-08-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL, NULL),
                ('16161616-1616-1616-1616-161616161616', 'a0000000-0000-0000-0000-000000000012', 'sample-refresh-token-staff-sky', '2026-08-21T10:30:00', NULL, '2026-06-21T10:30:00', '127.0.0.1', NULL, NULL);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [AccountRefreshTokens] WHERE [Id] IN ('16161616-1616-1616-1616-161616161612', '16161616-1616-1616-1616-161616161613', '16161616-1616-1616-1616-161616161614', '16161616-1616-1616-1616-161616161615', '16161616-1616-1616-1616-161616161616');
                DELETE FROM [AccountAccessTokens] WHERE [Id] IN ('15151515-1515-1515-1515-151515151512', '15151515-1515-1515-1515-151515151513', '15151515-1515-1515-1515-151515151514', '15151515-1515-1515-1515-151515151515', '15151515-1515-1515-1515-151515151516');
                DELETE FROM [MaintenanceTickets] WHERE [Id] IN ('14141414-1414-1414-1414-141414141412', '14141414-1414-1414-1414-141414141413', '14141414-1414-1414-1414-141414141414', '14141414-1414-1414-1414-141414141415');
                DELETE FROM [Technicians] WHERE [Id] IN ('13131313-1313-1313-1313-131313131312', '13131313-1313-1313-1313-131313131313', '13131313-1313-1313-1313-131313131314', '13131313-1313-1313-1313-131313131315');
                DELETE FROM [Invoices] WHERE [Id] IN ('12121212-1212-1212-1212-121212121212', '12121212-1212-1212-1212-121212121213', '12121212-1212-1212-1212-121212121214', '12121212-1212-1212-1212-121212121215');
                DELETE FROM [Contracts] WHERE [Id] IN ('ffffffff-ffff-ffff-ffff-fffffffffff2', 'ffffffff-ffff-ffff-ffff-fffffffffff3', 'ffffffff-ffff-ffff-ffff-fffffffffff4', 'ffffffff-ffff-ffff-ffff-fffffffffff5');
                DELETE FROM [Bookings] WHERE [Id] IN ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5');
                DELETE FROM [Brokers] WHERE [Id] IN ('dddddddd-dddd-dddd-dddd-ddddddddddd2', 'dddddddd-dddd-dddd-dddd-ddddddddddd3', 'dddddddd-dddd-dddd-dddd-ddddddddddd4', 'dddddddd-dddd-dddd-dddd-ddddddddddd5');
                DELETE FROM [Residents] WHERE [Id] IN ('cccccccc-cccc-cccc-cccc-ccccccccccc2', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 'cccccccc-cccc-cccc-cccc-ccccccccccc5', 'cccccccc-cccc-cccc-cccc-ccccccccccc6');
                DELETE FROM [AssetAssignments] WHERE [Id] IN ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5');
                DELETE FROM [Assets] WHERE [Id] IN ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5');
                DELETE FROM [RoomAvailabilities] WHERE [Id] IN ('99999999-9999-9999-9999-999999999993', '99999999-9999-9999-9999-999999999994', '99999999-9999-9999-9999-999999999995', '99999999-9999-9999-9999-999999999996', '99999999-9999-9999-9999-999999999997', '99999999-9999-9999-9999-999999999998', '99999999-9999-9999-9999-999999999999', '99999999-9999-9999-9999-99999999999a');
                DELETE FROM [RoomImages] WHERE [Id] IN ('88888888-8888-8888-8888-888888888882', '88888888-8888-8888-8888-888888888883', '88888888-8888-8888-8888-888888888884', '88888888-8888-8888-8888-888888888885', '88888888-8888-8888-8888-888888888886', '88888888-8888-8888-8888-888888888887', '88888888-8888-8888-8888-888888888888', '88888888-8888-8888-8888-888888888889', '88888888-8888-8888-8888-88888888888a');
                DELETE FROM [StaffUserPropertyMapping] WHERE [StaffUserId] IN ('55555555-5555-5555-5555-555555555552', '55555555-5555-5555-5555-555555555553', '55555555-5555-5555-5555-555555555554', '55555555-5555-5555-5555-555555555555');
                DELETE FROM [Rooms] WHERE [Id] IN ('77777777-7777-7777-7777-777777777773', '77777777-7777-7777-7777-777777777774', '77777777-7777-7777-7777-777777777775', '77777777-7777-7777-7777-777777777776', '77777777-7777-7777-7777-777777777777', '77777777-7777-7777-7777-777777777778', '77777777-7777-7777-7777-777777777779', '77777777-7777-7777-7777-77777777777a');
                DELETE FROM [Properties] WHERE [Id] IN ('66666666-6666-6666-6666-666666666662', '66666666-6666-6666-6666-666666666663', '66666666-6666-6666-6666-666666666664', '66666666-6666-6666-6666-666666666665');
                DELETE FROM [StaffUsers] WHERE [Id] IN ('55555555-5555-5555-5555-555555555552', '55555555-5555-5555-5555-555555555553', '55555555-5555-5555-5555-555555555554', '55555555-5555-5555-5555-555555555555');
                DELETE FROM [Organizations] WHERE [Id] IN ('33333333-3333-3333-3333-333333333332', '33333333-3333-3333-3333-333333333333', '33333333-3333-3333-3333-333333333334', '33333333-3333-3333-3333-333333333335');
                DELETE FROM [Accounts] WHERE [Id] IN ('a0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000003', 'a0000000-0000-0000-0000-000000000004', 'a0000000-0000-0000-0000-000000000011', 'a0000000-0000-0000-0000-000000000012', 'a0000000-0000-0000-0000-000000000013', 'a0000000-0000-0000-0000-000000000014', 'a0000000-0000-0000-0000-000000000021', 'a0000000-0000-0000-0000-000000000022', 'a0000000-0000-0000-0000-000000000023', 'a0000000-0000-0000-0000-000000000024', 'a0000000-0000-0000-0000-000000000025');
                DELETE FROM [RolePermissions] WHERE [RoleId] IN ('44444444-4444-4444-4444-444444444442', '44444444-4444-4444-4444-444444444443') OR [PermissionId] IN ('22222222-2222-2222-2222-222222222225', '22222222-2222-2222-2222-222222222226', '22222222-2222-2222-2222-222222222227', '22222222-2222-2222-2222-222222222228');
                DELETE FROM [Roles] WHERE [Id] IN ('44444444-4444-4444-4444-444444444442', '44444444-4444-4444-4444-444444444443');
                UPDATE [Roles]
                SET [RoleName] = N'Property Manager',
                    [RoleKey] = 'PROPERTY_MANAGER',
                    [Description] = N'Manages properties, rooms, contracts, and invoices.'
                WHERE [Id] = '44444444-4444-4444-4444-444444444441';
                DELETE FROM [Permissions] WHERE [Id] IN ('22222222-2222-2222-2222-222222222225', '22222222-2222-2222-2222-222222222226', '22222222-2222-2222-2222-222222222227', '22222222-2222-2222-2222-222222222228');
                DELETE FROM [SubscriptionPackages] WHERE [Id] IN ('11111111-1111-1111-1111-111111111113', '11111111-1111-1111-1111-111111111114', '11111111-1111-1111-1111-111111111115');
                """);

        }
    }
}
