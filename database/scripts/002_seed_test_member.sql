USE FifaWorldCupDb;
GO

IF NOT EXISTS (
    SELECT 1 FROM Members WHERE Username = 'test'
)
BEGIN
    INSERT INTO Members (
        Username,
        PasswordHash,
        PhoneNumber,
        CreditBalance
    )
    VALUES (
        'test',
        CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', '123456'), 2),
        '0123456789',
        0
    );
END
GO

SELECT * FROM Members;