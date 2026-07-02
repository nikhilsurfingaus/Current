using Current.Api.DTOs.Transactions;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionResponse>> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            var transaction = await _transactionService.TransferFundsAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetAll()
    {
        var transactions = await _transactionService.GetAllTransactionsAsync();
        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }
}
