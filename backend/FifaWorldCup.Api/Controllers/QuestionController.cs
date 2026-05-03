using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public QuestionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Question/list
        // GET: api/Question/list?memberId=1
        [HttpGet("list")]
        public IActionResult GetQuestionList([FromQuery] int? memberId)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT
                        q.Id,
                        q.PrizePoolType,
                        q.QuestionText,
                        q.IsLocked,
                        q.CreatedAt,

                        CASE
                            WHEN q.IsLocked = 0 THEN CAST(1 AS BIT)
                            WHEN mqu.Id IS NOT NULL THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT)
                        END AS IsUnlocked,

                        CASE
                            WHEN ma.Id IS NOT NULL THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT)
                        END AS HasSubmitted,

                        ma.AnswerText AS SubmittedAnswer,
                        ma.CreatedAt AS SubmittedAt,

                        CASE
                            WHEN 
                                (
                                    q.IsLocked = 0
                                    OR mqu.Id IS NOT NULL
                                )
                                AND ma.Id IS NULL
                            THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT)
                        END AS CanAnswer

                    FROM Questions q
                    LEFT JOIN MemberQuestionUnlocks mqu
                        ON q.Id = mqu.QuestionId
                        AND mqu.MemberId = @MemberId
                    LEFT JOIN MemberAnswers ma
                        ON q.Id = ma.QuestionId
                        AND ma.MemberId = @MemberId
                    ORDER BY q.Id ASC
                ";

                using var command = new SqlCommand(sql, connection);

                var memberIdParam = command.Parameters.Add("@MemberId", System.Data.SqlDbType.Int);
                memberIdParam.Value = memberId.HasValue ? memberId.Value : DBNull.Value;

                using var reader = command.ExecuteReader();

                var questions = new List<object>();

                while (reader.Read())
                {
                    var questionText = reader.GetString(reader.GetOrdinal("QuestionText"));
                    var isLocked = reader.GetBoolean(reader.GetOrdinal("IsLocked"));
                    var isUnlocked = reader.GetBoolean(reader.GetOrdinal("IsUnlocked"));
                    var hasSubmitted = reader.GetBoolean(reader.GetOrdinal("HasSubmitted"));
                    var canAnswer = reader.GetBoolean(reader.GetOrdinal("CanAnswer"));

                    var submittedAnswerOrdinal = reader.GetOrdinal("SubmittedAnswer");
                    var submittedAtOrdinal = reader.GetOrdinal("SubmittedAt");

                    string? submittedAnswer = reader.IsDBNull(submittedAnswerOrdinal)
                        ? null
                        : reader.GetString(submittedAnswerOrdinal);

                    DateTime? submittedAt = reader.IsDBNull(submittedAtOrdinal)
                        ? null
                        : reader.GetDateTime(submittedAtOrdinal);

                    questions.Add(new
                    {
                        id = reader.GetInt32(reader.GetOrdinal("Id")),
                        prizePoolType = reader.GetString(reader.GetOrdinal("PrizePoolType")),

                        questionText = isUnlocked || hasSubmitted
                            ? questionText
                            : "This question is locked.",

                        originalQuestionText = questionText,
                        isLocked,
                        isUnlocked,
                        hasSubmitted,
                        submittedAnswer,
                        submittedAt,
                        canAnswer,
                        createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Question list loaded successfully.",
                    data = questions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load question list.",
                    error = ex.Message
                });
            }
        }
    }

    public class QuestionListItem
    {
        public int Id { get; set; }

        public string PrizePoolType { get; set; } = string.Empty;

        public string QuestionText { get; set; } = string.Empty;

        public string OriginalQuestionText { get; set; } = string.Empty;

        public bool IsLocked { get; set; }

        public bool IsUnlocked { get; set; }

        public bool HasSubmitted { get; set; }

        public string? SubmittedAnswer { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool CanAnswer { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}