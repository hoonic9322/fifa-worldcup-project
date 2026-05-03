using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminDashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/admin/dashboard/summary
        [HttpGet("summary")]
        public IActionResult GetDashboardSummary()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT
                        (SELECT COUNT(*) FROM Members) AS TotalMembers,
                        (SELECT COUNT(*) FROM Questions) AS TotalQuestions,
                        (SELECT COUNT(*) FROM MemberAnswers) AS TotalSubmittedAnswers,
                        (SELECT COUNT(*) FROM MemberQuestionUnlocks) AS TotalUnlockRecords,
                        (SELECT COUNT(*) FROM CreditTransactions) AS TotalCreditTransactions
                ";

                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return Ok(new
                    {
                        status = "OK",
                        message = "Dashboard summary loaded successfully.",
                        data = new
                        {
                            totalMembers = 0,
                            totalQuestions = 0,
                            totalSubmittedAnswers = 0,
                            totalUnlockRecords = 0,
                            totalCreditTransactions = 0
                        }
                    });
                }

                var summary = new
                {
                    totalMembers = reader.GetInt32(reader.GetOrdinal("TotalMembers")),
                    totalQuestions = reader.GetInt32(reader.GetOrdinal("TotalQuestions")),
                    totalSubmittedAnswers = reader.GetInt32(reader.GetOrdinal("TotalSubmittedAnswers")),
                    totalUnlockRecords = reader.GetInt32(reader.GetOrdinal("TotalUnlockRecords")),
                    totalCreditTransactions = reader.GetInt32(reader.GetOrdinal("TotalCreditTransactions"))
                };

                return Ok(new
                {
                    status = "OK",
                    message = "Dashboard summary loaded successfully.",
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load dashboard summary.",
                    error = ex.Message
                });
            }
        }
    }
}