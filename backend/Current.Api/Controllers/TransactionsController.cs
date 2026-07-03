using Current.Api.DTOs.Transactions;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUserService;

    public TransactionsController(
        ITransactionService transactionService,
        ICurrentUserService currentUserService)
    {
        _transactionService = transactionService;
        _currentUserService = currentUserService;
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionResponse>> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var transaction = await _transactionService.TransferFundsAsync(request, currentUserId);
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
        var currentUserId = _currentUserService.GetCurrentUserId();
        var transactions = await _transactionService.GetAllTransactionsAsync(currentUserId);
        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, currentUserId);

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }
}
