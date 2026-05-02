using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public QuestionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetQuestionList()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, new
                {
                    status = "FAILED",
                    message = "Database connection is not configured."
                });
            }

            var questions = new List<QuestionListItem>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT 
                    Id,
                    PrizePoolType,
                    QuestionText,
                    IsLocked,
                    CreatedAt
                FROM Questions
                ORDER BY Id ASC;
            ";

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                questions.Add(new QuestionListItem
                {
                    Id = reader.GetInt32(0),
                    PrizePoolType = reader.GetString(1),
                    QuestionText = reader.GetString(2),
                    IsLocked = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return Ok(new
            {
                status = "OK",
                message = "Question list loaded successfully.",
                data = questions
            });
        }
    }

    public class QuestionListItem
    {
        public int Id { get; set; }

        public string PrizePoolType { get; set; } = string.Empty;

        public string QuestionText { get; set; } = string.Empty;

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}