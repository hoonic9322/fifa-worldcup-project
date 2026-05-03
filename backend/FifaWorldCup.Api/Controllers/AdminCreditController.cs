using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCreditController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminCreditController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/AdminCredit/add
        [HttpPost("add")]
        public IActionResult AddCredit([FromBody] CreditChangeRequest request)
        {
            return ChangeCredit(request, "ADD");
        }

        // POST: api/AdminCredit/deduct
        [HttpPost("deduct")]
        public IActionResult DeductCredit([FromBody] CreditChangeRequest request)
        {
            return ChangeCredit(request, "DEDUCT");
        }

        // GET: api/AdminCredit/transactions
        // GET: api/AdminCredit/transactions?memberId=1
        [HttpGet("transactions")]
        public IActionResult GetCreditTransactions([FromQuery] int? memberId)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                var sql = @"
                    SELECT
                        ct.Id,
                        ct.MemberId,
                        m.Username,
                        m.PhoneNumber,
                        ct.TransactionType,
                        ct.Amount,
                        ct.BalanceBefore,
                        ct.BalanceAfter,
                        ct.Remark,
                        ct.CreatedByAdminId,
                        au.Username AS AdminUsername,
                        ct.CreatedAt
                    FROM CreditTransactions ct
                    INNER JOIN Members m
                        ON ct.MemberId = m.Id
                    LEFT JOIN AdminUsers au
                        ON ct.CreatedByAdminId = au.Id
                    WHERE
                        (
                            @MemberId IS NULL
                            OR ct.MemberId = @MemberId
                        )
                    ORDER BY ct.CreatedAt DESC
                ";

                using var command = new SqlCommand(sql, connection);

                var memberIdParam = command.Parameters.Add("@MemberId", SqlDbType.Int);
                memberIdParam.Value = memberId.HasValue ? memberId.Value : DBNull.Value;

                using var reader = command.ExecuteReader();

                var transactions = new List<object>();

                while (reader.Read())
                {
                    transactions.Add(new
                    {
                        transactionId = reader.GetInt32(reader.GetOrdinal("Id")),
                        memberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                        username = reader.GetString(reader.GetOrdinal("Username")),
                        phoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        transactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                        amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        balanceBefore = reader.GetDecimal(reader.GetOrdinal("BalanceBefore")),
                        balanceAfter = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
                        remark = reader.IsDBNull(reader.GetOrdinal("Remark"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("Remark")),
                        createdByAdminId = reader.IsDBNull(reader.GetOrdinal("CreatedByAdminId"))
                            ? (int?)null
                            : reader.GetInt32(reader.GetOrdinal("CreatedByAdminId")),
                        adminUsername = reader.IsDBNull(reader.GetOrdinal("AdminUsername"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("AdminUsername")),
                        createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    message = "Credit transaction list loaded successfully.",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to load credit transactions.",
                    error = ex.Message
                });
            }
        }

        private IActionResult ChangeCredit(CreditChangeRequest request, string transactionType)
        {
            SqlTransaction? dbTransaction = null;

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

                if (request.MemberId <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "MemberId is required."
                    });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Amount must be greater than 0."
                    });
                }

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                dbTransaction = connection.BeginTransaction();

                var selectSql = @"
                    SELECT
                        Id,
                        CreditBalance
                    FROM Members WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = @MemberId
                ";

                decimal balanceBefore;

                using (var selectCommand = new SqlCommand(selectSql, connection, dbTransaction))
                {
                    selectCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                    using var reader = selectCommand.ExecuteReader();

                    if (!reader.Read())
                    {
                        dbTransaction.Rollback();

                        return NotFound(new
                        {
                            status = "ERROR",
                            message = "Member not found."
                        });
                    }

                    balanceBefore = reader.GetDecimal(reader.GetOrdinal("CreditBalance"));
                }

                decimal balanceAfter;

                if (transactionType == "ADD")
                {
                    balanceAfter = balanceBefore + request.Amount;
                }
                else
                {
                    if (balanceBefore < request.Amount)
                    {
                        dbTransaction.Rollback();

                        return BadRequest(new
                        {
                            status = "ERROR",
                            message = "Insufficient credit balance."
                        });
                    }

                    balanceAfter = balanceBefore - request.Amount;
                }

                var updateSql = @"
                    UPDATE Members
                    SET CreditBalance = @BalanceAfter
                    WHERE Id = @MemberId
                ";

                using (var updateCommand = new SqlCommand(updateSql, connection, dbTransaction))
                {
                    updateCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                    updateCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                    updateCommand.ExecuteNonQuery();
                }

                var insertLogSql = @"
                    INSERT INTO CreditTransactions
                    (
                        MemberId,
                        TransactionType,
                        Amount,
                        BalanceBefore,
                        BalanceAfter,
                        Remark,
                        CreatedByAdminId,
                        CreatedAt
                    )
                    VALUES
                    (
                        @MemberId,
                        @TransactionType,
                        @Amount,
                        @BalanceBefore,
                        @BalanceAfter,
                        @Remark,
                        @CreatedByAdminId,
                        GETDATE()
                    )
                ";

                using (var insertLogCommand = new SqlCommand(insertLogSql, connection, dbTransaction))
                {
                    insertLogCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    insertLogCommand.Parameters.AddWithValue("@TransactionType", transactionType);
                    insertLogCommand.Parameters.AddWithValue("@Amount", request.Amount);
                    insertLogCommand.Parameters.AddWithValue("@BalanceBefore", balanceBefore);
                    insertLogCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                    insertLogCommand.Parameters.AddWithValue(
                        "@Remark",
                        string.IsNullOrWhiteSpace(request.Remark) ? DBNull.Value : request.Remark.Trim()
                    );
                    insertLogCommand.Parameters.AddWithValue(
                        "@CreatedByAdminId",
                        request.AdminId <= 0 ? DBNull.Value : request.AdminId
                    );

                    insertLogCommand.ExecuteNonQuery();
                }

                dbTransaction.Commit();

                return Ok(new
                {
                    status = "OK",
                    message = transactionType == "ADD"
                        ? "Credit added successfully."
                        : "Credit deducted successfully.",
                    data = new
                    {
                        memberId = request.MemberId,
                        transactionType,
                        amount = request.Amount,
                        balanceBefore,
                        balanceAfter
                    }
                });
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction?.Rollback();
                }
                catch
                {
                    // Ignore rollback exception
                }

                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = "Failed to change credit.",
                    error = ex.Message
                });
            }
        }
    }

    public class CreditChangeRequest
    {
        public int MemberId { get; set; }

        public decimal Amount { get; set; }

        public string Remark { get; set; } = string.Empty;

        public int AdminId { get; set; }
    }
}