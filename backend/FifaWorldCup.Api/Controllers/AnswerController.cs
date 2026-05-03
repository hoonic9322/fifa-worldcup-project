using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AnswerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/Answer/submit
        [HttpPost("submit")]
        public IActionResult SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Invalid request."
                    });
                }

                if (request.MemberId <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "MemberId is required."
                    });
                }

                if (request.QuestionId <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "QuestionId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.AnswerText))
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Answer text is required."
                    });
                }

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                // 1. Check question exists
                var questionCheckSql = @"
                    SELECT COUNT(1)
                    FROM Questions
                    WHERE Id = @QuestionId
                ";

                using (var questionCheckCommand = new SqlCommand(questionCheckSql, connection))
                {
                    questionCheckCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                    var questionCount = (int)questionCheckCommand.ExecuteScalar();

                    if (questionCount <= 0)
                    {
                        return BadRequest(new
                        {
                            status = "ERROR",
                            message = "Question not found."
                        });
                    }
                }

                // 2. Check whether this question is unlocked for this member
                var unlockCheckSql = @"
                    SELECT
                        CASE
                            WHEN q.IsLocked = 0 THEN CAST(1 AS BIT)
                            WHEN mqu.Id IS NOT NULL THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT)
                        END AS CanSubmit
                    FROM Questions q
                    LEFT JOIN MemberQuestionUnlocks mqu
                        ON q.Id = mqu.QuestionId
                        AND mqu.MemberId = @MemberId
                    WHERE q.Id = @QuestionId
                ";

                using (var unlockCheckCommand = new SqlCommand(unlockCheckSql, connection))
                {
                    unlockCheckCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    unlockCheckCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                    var canSubmitObj = unlockCheckCommand.ExecuteScalar();

                    if (canSubmitObj == null || canSubmitObj == DBNull.Value || !(bool)canSubmitObj)
                    {
                        return BadRequest(new
                        {
                            status = "ERROR",
                            message = "This question is locked. Please unlock it before submitting an answer."
                        });
                    }
                }

                // 3. Check duplicate answer by MemberId + QuestionId
                var checkSql = @"
                    SELECT COUNT(1)
                    FROM MemberAnswers
                    WHERE MemberId = @MemberId
                      AND QuestionId = @QuestionId
                ";

                using (var checkCommand = new SqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    checkCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                    var existingCount = (int)checkCommand.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        return BadRequest(new
                        {
                            status = "ERROR",
                            message = "You have already submitted an answer for this question."
                        });
                    }
                }

                // 4. Insert answer
                var insertSql = @"
                    INSERT INTO MemberAnswers
                    (
                        MemberId,
                        QuestionId,
                        AnswerText,
                        CreatedAt
                    )
                    OUTPUT INSERTED.Id, INSERTED.CreatedAt
                    VALUES
                    (
                        @MemberId,
                        @QuestionId,
                        @AnswerText,
                        GETDATE()
                    )
                ";

                using var insertCommand = new SqlCommand(insertSql, connection);
                insertCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                insertCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);
                insertCommand.Parameters.AddWithValue("@AnswerText", request.AnswerText.Trim());

                using var reader = insertCommand.ExecuteReader();

                int answerId = 0;
                DateTime createdAt = DateTime.Now;

                if (reader.Read())
                {
                    answerId = reader.GetInt32(reader.GetOrdinal("Id"));
                    createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Answer submitted successfully.",
                    data = new
                    {
                        id = answerId,
                        memberId = request.MemberId,
                        questionId = request.QuestionId,
                        answerText = request.AnswerText.Trim(),
                        submittedAt = createdAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to submit answer.",
                    error = ex.Message
                });
            }
        }

        // GET: api/Answer/member/1
        [HttpGet("member/{memberId}")]
        public IActionResult GetSubmittedAnswer(int memberId)
        {
            try
            {
                if (memberId <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Invalid memberId."
                    });
                }

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT TOP 1
                        Id,
                        MemberId,
                        QuestionId,
                        AnswerText,
                        CreatedAt
                    FROM MemberAnswers
                    WHERE MemberId = @MemberId
                    ORDER BY CreatedAt DESC
                ";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@MemberId", memberId);

                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return Ok(new
                    {
                        status = "OK",
                        message = "No submitted answer found.",
                        data = (object?)null
                    });
                }

                var answer = new
                {
                    id = reader.GetInt32(reader.GetOrdinal("Id")),
                    memberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                    questionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                    answerText = reader.GetString(reader.GetOrdinal("AnswerText")),
                    submittedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };

                return Ok(new
                {
                    status = "OK",
                    message = "Submitted answer loaded successfully.",
                    data = answer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load submitted answer.",
                    error = ex.Message
                });
            }
        }
    }

    public class SubmitAnswerRequest
    {
        public int MemberId { get; set; }

        public int QuestionId { get; set; }

        public string AnswerText { get; set; } = string.Empty;
    }
}