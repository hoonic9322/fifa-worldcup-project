using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAnswerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminAnswerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/AdminAnswer/list
        [HttpGet("list")]
        public IActionResult GetAnswerList()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT
                        ma.Id AS AnswerId,
                        ma.MemberId,
                        m.Username,
                        m.PhoneNumber,
                        ma.QuestionId,
                        q.PrizePoolType,
                        q.QuestionText,
                        ma.AnswerText,
                        ma.CreatedAt AS SubmittedAt
                    FROM MemberAnswers ma
                    INNER JOIN Members m
                        ON ma.MemberId = m.Id
                    INNER JOIN Questions q
                        ON ma.QuestionId = q.Id
                    ORDER BY ma.CreatedAt DESC, ma.Id DESC
                ";

                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                var answers = new List<object>();

                while (reader.Read())
                {
                    answers.Add(new
                    {
                        answerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                        memberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                        username = reader.GetString(reader.GetOrdinal("Username")),
                        phoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        questionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                        prizePoolType = reader.GetString(reader.GetOrdinal("PrizePoolType")),
                        questionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        answerText = reader.GetString(reader.GetOrdinal("AnswerText")),
                        submittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt"))
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Answer list loaded successfully.",
                    data = answers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load answer list.",
                    error = ex.Message
                });
            }
        }
    }
}