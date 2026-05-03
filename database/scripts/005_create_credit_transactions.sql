USE FifaWorldCupDb;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = 'CreditTransactions'
)
BEGIN
    CREATE TABLE CreditTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        TransactionType NVARCHAR(20) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        BalanceBefore DECIMAL(18,2) NOT NULL,
        BalanceAfter DECIMAL(18,2) NOT NULL,
        Remark NVARCHAR(500) NULL,
        CreatedByAdminId INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_CreditTransactions_Members
            FOREIGN KEY (MemberId) REFERENCES Members(Id)
    );
END
GO

SELECT
    Id,
    MemberId,
    TransactionType,
    Amount,
    BalanceBefore,
    BalanceAfter,
    Remark,
    CreatedByAdminId,
    CreatedAt
FROM CreditTransactions;
GO