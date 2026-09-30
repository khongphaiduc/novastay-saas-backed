INSERT INTO SubscriptionPackages (
    Id, 
    PackageName, 
    PackageKey, 
    PriceMonthly, 
    MaxProperties, 
    MaxRooms, 
    TokenGiftMonthly, 
    AllowCustomRoles, 
    AllowAiFeatures, 
    CreatedAt
) VALUES 
(
    NEWID(), 
    N'Gói Dùng Thử', 
    'TRIAL', 
    0, 
    5, 
    20, 
    100, 
    1, 
    1, 
    GETDATE()
),
(
    NEWID(), 
    N'Gói Cơ Bản', 
    'BASIC', 
    299000, 
    10, 
    50, 
    200, 
    0, 
    0, 
    GETDATE()
),
(
    NEWID(), 
    N'Gói Nâng Cao', 
    'PRO', 
    799000, 
    50, 
    200, 
    500, 
    1, 
    1, 
    GETDATE()
);
