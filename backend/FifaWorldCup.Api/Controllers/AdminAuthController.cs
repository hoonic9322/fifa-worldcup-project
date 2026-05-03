using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/AdminAuth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
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

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Username is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Password is required."
                    });
                }

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT TOP 1
                        Id,
                        Username,
                        Password,
                        DisplayName,
                        IsActive
                    FROM AdminUsers
                    WHERE Username = @Username
                ";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", request.Username.Trim());

                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return Unauthorized(new
                    {
                        status = "ERROR",
                        message = "Invalid username or password."
                    });
                }

                var adminId = reader.GetInt32(reader.GetOrdinal("Id"));
                var username = reader.GetString(reader.GetOrdinal("Username"));
                var password = reader.GetString(reader.GetOrdinal("Password"));
                var displayName = reader.IsDBNull(reader.GetOrdinal("DisplayName"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("DisplayName"));
                var isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));

                if (!isActive)
                {
                    return Unauthorized(new
                    {
                        status = "ERROR",
                        message = "Admin account is inactive."
                    });
                }

                if (password != request.Password)
                {
                    return Unauthorized(new
                    {
                        status = "ERROR",
                        message = "Invalid username or password."
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Admin login successful.",
                    data = new
                    {
                        adminId,
                        username,
                        displayName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to login admin.",
                    error = ex.Message
                });
            }
        }
    }

    public class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}