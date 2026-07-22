using Current.Api.DTOs.Loans;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ICurrentUserService _currentUserService;

    public LoansController(ILoanService loanService, ICurrentUserService currentUserService)
    {
        _loanService = loanService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoanResponse>>> GetAll()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var loans = await _loanService.GetUserLoansAsync(currentUserId);
        return Ok(loans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanResponse>> GetById(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var loan = await _loanService.GetUserLoanByIdAsync(id, currentUserId);

        if (loan is null)
        {
            return NotFound();
        }

        return Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanResponse>> Create([FromBody] CreateLoanRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var loan = await _loanService.CreateLoanRequestAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<LoanResponse>> Cancel(Guid id)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var loan = await _loanService.CancelLoanRequestAsync(id, currentUserId);

            if (loan is null)
            {
                return NotFound();
            }

            return Ok(loan);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/repay")]
    public async Task<ActionResult<LoanResponse>> Repay(Guid id, [FromBody] RepayLoanRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var loan = await _loanService.RepayLoanAsync(id, request, currentUserId);
            return Ok(loan);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}/repayments")]
    public async Task<ActionResult<IReadOnlyList<LoanRepaymentResponse>>> GetRepayments(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var loan = await _loanService.GetUserLoanByIdAsync(id, currentUserId);

        if (loan is null)
        {
            return NotFound();
        }

        var repayments = await _loanService.GetRepaymentHistoryAsync(id, currentUserId);
        return Ok(repayments);
    }
}
