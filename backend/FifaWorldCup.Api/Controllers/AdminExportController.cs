using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminExportController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminExportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/AdminExport/answers
        [HttpGet("answers")]
        public IActionResult ExportAnswers()
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
                        q.PrizePoolType,
                        ma.QuestionId,
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

                var csv = new StringBuilder();

                csv.AppendLine("Answer ID,Member ID,Username,Phone Number,Prize Pool,Question ID,Question,Answer,Submitted Time");

                while (reader.Read())
                {
                    var answerId = reader.GetInt32(reader.GetOrdinal("AnswerId"));
                    var memberId = reader.GetInt32(reader.GetOrdinal("MemberId"));
                    var username = reader.GetString(reader.GetOrdinal("Username"));
                    var phoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("PhoneNumber"));
                    var prizePoolType = reader.GetString(reader.GetOrdinal("PrizePoolType"));
                    var questionId = reader.GetInt32(reader.GetOrdinal("QuestionId"));
                    var questionText = reader.GetString(reader.GetOrdinal("QuestionText"));
                    var answerText = reader.GetString(reader.GetOrdinal("AnswerText"));
                    var submittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt"));

                    csv.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(answerId.ToString()),
                        EscapeCsv(memberId.ToString()),
                        EscapeCsv(username),
                       EscapeCsv($"=\"{phoneNumber}\""),
                        EscapeCsv(prizePoolType),
                        EscapeCsv(questionId.ToString()),
                        EscapeCsv(questionText),
                        EscapeCsv(answerText),
                        EscapeCsv(submittedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    }));
                }

                var bytes = Encoding.UTF8.GetPreamble()
                    .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
                    .ToArray();

                var fileName = $"fifa-worldcup-answers-{DateTime.Now:yyyyMMddHHmmss}.csv";

                return File(bytes, "text/csv; charset=utf-8", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to export answers.",
                    error = ex.Message
                });
            }
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var escapedValue = value.Replace("\"", "\"\"");

            if (escapedValue.Contains(",") ||
                escapedValue.Contains("\"") ||
                escapedValue.Contains("\n") ||
                escapedValue.Contains("\r"))
            {
                return $"\"{escapedValue}\"";
            }

            return escapedValue;
        }
    }
}