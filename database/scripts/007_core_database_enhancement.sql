/*
    007_core_database_enhancement.sql

    Purpose:
    - Add unique constraints for important business rules
    - Prevent duplicate answers for the same member and question
    - Prevent duplicate unlock records for the same member and question
    - Improve search, answer list, export, and credit transaction query performance

    Notes:
    - Safe to run multiple times because every index creation checks IF NOT EXISTS
    - Before running this script, check duplicate records in:
        1. MemberAnswers by MemberId + QuestionId
        2. MemberQuestionUnlocks by MemberId + QuestionId
*/

-- =========================================================
-- 1. Prevent duplicate answer for same member + same question
-- Business Rule:
-- One member can answer multiple questions.
-- But the same member cannot submit more than one answer for the same question.
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_MemberAnswers_MemberId_QuestionId'
      AND object_id = OBJECT_ID('MemberAnswers')
)
BEGIN
    CREATE UNIQUE INDEX UX_MemberAnswers_MemberId_QuestionId
    ON MemberAnswers (MemberId, QuestionId);
END;
GO

-- =========================================================
-- 2. Prevent duplicate unlock for same member + same question
-- Business Rule:
-- The same member should not unlock the same question multiple times.
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_MemberQuestionUnlocks_MemberId_QuestionId'
      AND object_id = OBJECT_ID('MemberQuestionUnlocks')
)
BEGIN
    CREATE UNIQUE INDEX UX_MemberQuestionUnlocks_MemberId_QuestionId
    ON MemberQuestionUnlocks (MemberId, QuestionId);
END;
GO

-- =========================================================
-- 3. Improve member username search
-- Used by:
-- Admin Member List search by username
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Members_Username'
      AND object_id = OBJECT_ID('Members')
)
BEGIN
    CREATE INDEX IX_Members_Username
    ON Members (Username);
END;
GO

-- =========================================================
-- 4. Improve member phone number search
-- Used by:
-- Admin Member List search by phone number
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Members_PhoneNumber'
      AND object_id = OBJECT_ID('Members')
)
BEGIN
    CREATE INDEX IX_Members_PhoneNumber
    ON Members (PhoneNumber);
END;
GO

-- =========================================================
-- 5. Improve admin answer list and export sorting
-- Used by:
-- Admin Answer List
-- Admin Export Answer CSV
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MemberAnswers_CreatedAt'
      AND object_id = OBJECT_ID('MemberAnswers')
)
BEGIN
    CREATE INDEX IX_MemberAnswers_CreatedAt
    ON MemberAnswers (CreatedAt DESC, Id DESC);
END;
GO

-- =========================================================
-- 6. Improve answer lookup by member
-- Used by:
-- Member answer lookup
-- Member answered count / latest answer time
-- Admin member list statistics
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MemberAnswers_MemberId_CreatedAt'
      AND object_id = OBJECT_ID('MemberAnswers')
)
BEGIN
    CREATE INDEX IX_MemberAnswers_MemberId_CreatedAt
    ON MemberAnswers (MemberId, CreatedAt DESC, Id DESC);
END;
GO

-- =========================================================
-- 7. Improve credit transaction log query and member filter
-- Used by:
-- Admin Credit Transaction Log
-- Filter by Member ID
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CreditTransactions_MemberId_CreatedAt'
      AND object_id = OBJECT_ID('CreditTransactions')
)
BEGIN
    CREATE INDEX IX_CreditTransactions_MemberId_CreatedAt
    ON CreditTransactions (MemberId, CreatedAt DESC, Id DESC);
END;
GO

-- =========================================================
-- 8. Improve question listing by prize pool and lock status
-- Used by:
-- Member Question Page
-- Public Campaign / Prize Pool filtering later
-- =========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Questions_PrizePoolType_IsLocked'
      AND object_id = OBJECT_ID('Questions')
)
BEGIN
    CREATE INDEX IX_Questions_PrizePoolType_IsLocked
    ON Questions (PrizePoolType, IsLocked);
END;
GO