using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace FifaWorldCup.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MemberAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MemberLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "Username and password are required."
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

            var passwordHash = HashPassword(request.Password);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT 
                    Id,
                    Username,
                    PhoneNumber,
                    CreditBalance
                FROM Members
                WHERE Username = @Username
                  AND UPPER(PasswordHash) = UPPER(@PasswordHash);
            ";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", request.Username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return Unauthorized(new
                {
                    status = "FAILED",
                    message = "Invalid username or password."
                });
            }

            return Ok(new
            {
                status = "OK",
                message = "Login successful.",
                member = new
                {
                    id = reader.GetInt32(0),
                    username = reader.GetString(1),
                    phoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                    creditBalance = reader.GetDecimal(3)
                }
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }
    }

    public class MemberLoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}