USE FifaWorldCupDb;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = 'MemberQuestionUnlocks'
)
BEGIN
    CREATE TABLE MemberQuestionUnlocks (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        QuestionId INT NOT NULL,
        CreditCost DECIMAL(18,2) NOT NULL,
        BalanceBefore DECIMAL(18,2) NOT NULL,
        BalanceAfter DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_MemberQuestionUnlocks_Members
            FOREIGN KEY (MemberId) REFERENCES Members(Id),

        CONSTRAINT FK_MemberQuestionUnlocks_Questions
            FOREIGN KEY (QuestionId) REFERENCES Questions(Id),

        CONSTRAINT UQ_MemberQuestionUnlocks_Member_Question
            UNIQUE (MemberId, QuestionId)
    );
END
GO

SELECT
    Id,
    MemberId,
    QuestionId,
    CreditCost,
    BalanceBefore,
    BalanceAfter,
    CreatedAt
FROM MemberQuestionUnlocks;
GO