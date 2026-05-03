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

                // Check duplicate answer
                var checkSql = @"
                    SELECT COUNT(1)
                    FROM MemberAnswers
                    WHERE MemberId = @MemberId
                ";

                using (var checkCommand = new SqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                    var existingCount = (int)checkCommand.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        return BadRequest(new
                        {
                            status = "ERROR",
                            message = "You have already submitted an answer."
                        });
                    }
                }

                // Insert answer
                var insertSql = @"
                    INSERT INTO MemberAnswers
                    (
                        MemberId,
                        QuestionId,
                        AnswerText,
                        CreatedAt
                    )
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

                insertCommand.ExecuteNonQuery();

                return Ok(new
                {
                    status = "OK",
                    message = "Answer submitted successfully."
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