USE FifaWorldCupDb;
GO

IF NOT EXISTS (
    SELECT 1 FROM Questions WHERE QuestionText = 'Which country will win the FIFA World Cup?'
)
BEGIN
    INSERT INTO Questions (
        PrizePoolType,
        QuestionText,
        IsLocked
    )
    VALUES
    (
        '80K',
        'Which country will win the FIFA World Cup?',
        0
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM Questions WHERE QuestionText = 'Who will be the top scorer of the tournament?'
)
BEGIN
    INSERT INTO Questions (
        PrizePoolType,
        QuestionText,
        IsLocked
    )
    VALUES
    (
        '80K',
        'Who will be the top scorer of the tournament?',
        0
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM Questions WHERE QuestionText = 'Which team will reach the final?'
)
BEGIN
    INSERT INTO Questions (
        PrizePoolType,
        QuestionText,
        IsLocked
    )
    VALUES
    (
        '20K',
        'Which team will reach the final?',
        0
    );
END
GO

SELECT * FROM Questions;