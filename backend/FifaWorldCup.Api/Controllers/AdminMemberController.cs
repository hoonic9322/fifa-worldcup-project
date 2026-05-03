using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminMemberController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminMemberController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/AdminMember/list
        [HttpGet("list")]
        public IActionResult GetMemberList()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT
                        m.Id,
                        m.Username,
                        m.PhoneNumber,
                        m.CreditBalance,
                        m.CreatedAt,
                        COUNT(ma.Id) AS AnsweredCount,
                        MAX(ma.CreatedAt) AS LastAnswerTime
                    FROM Members m
                    LEFT JOIN MemberAnswers ma
                        ON m.Id = ma.MemberId
                    GROUP BY
                        m.Id,
                        m.Username,
                        m.PhoneNumber,
                        m.CreditBalance,
                        m.CreatedAt
                    ORDER BY m.Id DESC
                ";

                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                var members = new List<object>();

                while (reader.Read())
                {
                    members.Add(new
                    {
                        memberId = reader.GetInt32(reader.GetOrdinal("Id")),
                        username = reader.GetString(reader.GetOrdinal("Username")),
                        phoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        creditBalance = reader.GetDecimal(reader.GetOrdinal("CreditBalance")),
                        createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        answeredCount = reader.GetInt32(reader.GetOrdinal("AnsweredCount")),
                        lastAnswerTime = reader.IsDBNull(reader.GetOrdinal("LastAnswerTime"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("LastAnswerTime"))
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Member list loaded successfully.",
                    data = members
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load member list.",
                    error = ex.Message
                });
            }
        }
    }
}