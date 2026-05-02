using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AnswerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            if (request.MemberId <= 0)
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "MemberId is required."
                });
            }

            if (request.QuestionId <= 0)
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "QuestionId is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.AnswerText))
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "AnswerText is required."
                });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, new
                {
                    status = "FAILED",
                    message = "Database connection is not configured."
                });
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // 1. Check member exists
            const string checkMemberSql = @"
                SELECT COUNT(1)
                FROM Members
                WHERE Id = @MemberId;
            ";

            await using (var checkMemberCommand = new SqlCommand(checkMemberSql, connection))
            {
                checkMemberCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                var memberCount = (int)await checkMemberCommand.ExecuteScalarAsync();

                if (memberCount == 0)
                {
                    return NotFound(new
                    {
                        status = "FAILED",
                        message = "Member not found."
                    });
                }
            }

            // 2. Check question exists and is not locked
            const string checkQuestionSql = @"
                SELECT IsLocked
                FROM Questions
                WHERE Id = @QuestionId;
            ";

            bool isLocked;

            await using (var checkQuestionCommand = new SqlCommand(checkQuestionSql, connection))
            {
                checkQuestionCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                var result = await checkQuestionCommand.ExecuteScalarAsync();

                if (result == null)
                {
                    return NotFound(new
                    {
                        status = "FAILED",
                        message = "Question not found."
                    });
                }

                isLocked = (bool)result;
            }

            if (isLocked)
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "This question is locked."
                });
            }

            // 3. Prevent duplicate answer for same member + question
            const string checkExistingAnswerSql = @"
                SELECT COUNT(1)
                FROM MemberAnswers
                WHERE MemberId = @MemberId
                  AND QuestionId = @QuestionId;
            ";

            await using (var checkAnswerCommand = new SqlCommand(checkExistingAnswerSql, connection))
            {
                checkAnswerCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                checkAnswerCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                var answerCount = (int)await checkAnswerCommand.ExecuteScalarAsync();

                if (answerCount > 0)
                {
                    return BadRequest(new
                    {
                        status = "FAILED",
                        message = "You have already answered this question."
                    });
                }
            }

            // 4. Insert answer
            const string insertSql = @"
                INSERT INTO MemberAnswers (
                    MemberId,
                    QuestionId,
                    AnswerText
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @MemberId,
                    @QuestionId,
                    @AnswerText
                );
            ";

            int answerId;

            await using (var insertCommand = new SqlCommand(insertSql, connection))
            {
                insertCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                insertCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);
                insertCommand.Parameters.AddWithValue("@AnswerText", request.AnswerText.Trim());

                answerId = (int)await insertCommand.ExecuteScalarAsync();
            }

            return Ok(new
            {
                status = "OK",
                message = "Answer submitted successfully.",
                answerId
            });
        }
    }

    public class SubmitAnswerRequest
    {
        public int MemberId { get; set; }

        public int QuestionId { get; set; }

        public string AnswerText { get; set; } = string.Empty;
    }
}