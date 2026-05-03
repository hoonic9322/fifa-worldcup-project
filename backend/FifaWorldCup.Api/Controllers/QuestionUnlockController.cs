using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FifaWorldCup.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionUnlockController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public QuestionUnlockController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/QuestionUnlock/unlock
        [HttpPost("unlock")]
        public IActionResult UnlockQuestion([FromBody] UnlockQuestionRequest request)
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

                if (request.QuestionId <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "QuestionId is required."
                    });
                }

                if (request.CreditCost <= 0)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "CreditCost must be greater than 0."
                    });
                }

                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                dbTransaction = connection.BeginTransaction();

                // 1. Check member and lock member balance row
                var memberSql = @"
                    SELECT
                        Id,
                        CreditBalance
                    FROM Members WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = @MemberId
                ";

                decimal balanceBefore;

                using (var memberCommand = new SqlCommand(memberSql, connection, dbTransaction))
                {
                    memberCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                    using var reader = memberCommand.ExecuteReader();

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

                // 2. Check question
                var questionSql = @"
                    SELECT
                        Id,
                        IsLocked
                    FROM Questions
                    WHERE Id = @QuestionId
                ";

                bool isLocked;

                using (var questionCommand = new SqlCommand(questionSql, connection, dbTransaction))
                {
                    questionCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                    using var reader = questionCommand.ExecuteReader();

                    if (!reader.Read())
                    {
                        dbTransaction.Rollback();

                        return NotFound(new
                        {
                            status = "ERROR",
                            message = "Question not found."
                        });
                    }

                    isLocked = reader.GetBoolean(reader.GetOrdinal("IsLocked"));
                }

                // 3. If question is not locked, no need to unlock
                if (!isLocked)
                {
                    dbTransaction.Rollback();

                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Question is not locked."
                    });
                }

                // 4. Check already unlocked
                var existingUnlockSql = @"
                    SELECT TOP 1 Id
                    FROM MemberQuestionUnlocks
                    WHERE MemberId = @MemberId
                      AND QuestionId = @QuestionId
                ";

                using (var existingCommand = new SqlCommand(existingUnlockSql, connection, dbTransaction))
                {
                    existingCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    existingCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);

                    var existingId = existingCommand.ExecuteScalar();

                    if (existingId != null)
                    {
                        dbTransaction.Rollback();

                        return Ok(new
                        {
                            status = "OK",
                            message = "Question already unlocked.",
                            data = new
                            {
                                memberId = request.MemberId,
                                questionId = request.QuestionId,
                                alreadyUnlocked = true
                            }
                        });
                    }
                }

                // 5. Check credit balance
                if (balanceBefore < request.CreditCost)
                {
                    dbTransaction.Rollback();

                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Insufficient credit balance."
                    });
                }

                var balanceAfter = balanceBefore - request.CreditCost;

                // 6. Deduct member credit
                var updateMemberSql = @"
                    UPDATE Members
                    SET CreditBalance = @BalanceAfter
                    WHERE Id = @MemberId
                ";

                using (var updateMemberCommand = new SqlCommand(updateMemberSql, connection, dbTransaction))
                {
                    updateMemberCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                    updateMemberCommand.Parameters.AddWithValue("@MemberId", request.MemberId);

                    updateMemberCommand.ExecuteNonQuery();
                }

                // 7. Insert unlock record
                var insertUnlockSql = @"
                    INSERT INTO MemberQuestionUnlocks
                    (
                        MemberId,
                        QuestionId,
                        CreditCost,
                        BalanceBefore,
                        BalanceAfter,
                        CreatedAt
                    )
                    VALUES
                    (
                        @MemberId,
                        @QuestionId,
                        @CreditCost,
                        @BalanceBefore,
                        @BalanceAfter,
                        GETDATE()
                    )
                ";

                using (var insertUnlockCommand = new SqlCommand(insertUnlockSql, connection, dbTransaction))
                {
                    insertUnlockCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    insertUnlockCommand.Parameters.AddWithValue("@QuestionId", request.QuestionId);
                    insertUnlockCommand.Parameters.AddWithValue("@CreditCost", request.CreditCost);
                    insertUnlockCommand.Parameters.AddWithValue("@BalanceBefore", balanceBefore);
                    insertUnlockCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);

                    insertUnlockCommand.ExecuteNonQuery();
                }

                // 8. Insert credit transaction log
                var insertCreditLogSql = @"
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
                        NULL,
                        GETDATE()
                    )
                ";

                using (var insertCreditLogCommand = new SqlCommand(insertCreditLogSql, connection, dbTransaction))
                {
                    insertCreditLogCommand.Parameters.AddWithValue("@MemberId", request.MemberId);
                    insertCreditLogCommand.Parameters.AddWithValue("@TransactionType", "UNLOCK");
                    insertCreditLogCommand.Parameters.AddWithValue("@Amount", request.CreditCost);
                    insertCreditLogCommand.Parameters.AddWithValue("@BalanceBefore", balanceBefore);
                    insertCreditLogCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                    insertCreditLogCommand.Parameters.AddWithValue(
                        "@Remark",
                        $"Unlock question ID {request.QuestionId}"
                    );

                    insertCreditLogCommand.ExecuteNonQuery();
                }

                dbTransaction.Commit();

                return Ok(new
                {
                    status = "OK",
                    message = "Question unlocked successfully.",
                    data = new
                    {
                        memberId = request.MemberId,
                        questionId = request.QuestionId,
                        creditCost = request.CreditCost,
                        balanceBefore,
                        balanceAfter,
                        alreadyUnlocked = false
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
                    message = "Failed to unlock question.",
                    error = ex.Message
                });
            }
        }
    }

    public class UnlockQuestionRequest
    {
        public int MemberId { get; set; }

        public int QuestionId { get; set; }

        public decimal CreditCost { get; set; }
    }
}