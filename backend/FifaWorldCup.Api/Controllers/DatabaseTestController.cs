using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DatabaseTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return BadRequest(new
                {
                    status = "FAILED",
                    message = "DefaultConnection is not configured."
                });
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SELECT DB_NAME()", connection);
            var databaseName = await command.ExecuteScalarAsync();

            return Ok(new
            {
                status = "OK",
                message = "Database connection successful",
                database = databaseName
            });
        }
    }
}