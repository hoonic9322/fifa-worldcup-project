USE FifaWorldCupDb;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = 'AdminUsers'
)
BEGIN
    CREATE TABLE AdminUsers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL,
        DisplayName NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM AdminUsers
    WHERE Username = 'admin'
)
BEGIN
    INSERT INTO AdminUsers
    (
        Username,
        Password,
        DisplayName,
        IsActive,
        CreatedAt
    )
    VALUES
    (
        'admin',
        '123456',
        'System Admin',
        1,
        GETDATE()
    );
END
GO

SELECT
    Id,
    Username,
    Password,
    DisplayName,
    IsActive,
    CreatedAt
FROM AdminUsers;
GO