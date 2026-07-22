using Current.Api.DTOs.Branches;
using Current.Api.DTOs.Loans;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize(Roles = nameof(Common.Enums.UserRole.Admin))]
[ApiController]
[Route("branch")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ILoanService _loanService;

    public BranchController(IBranchService branchService, ILoanService loanService)
    {
        _branchService = branchService;
        _loanService = loanService;
    }

    [HttpGet("treasury")]
    public async Task<ActionResult<BranchTreasuryResponse>> GetTreasury()
    {
        var treasury = await _branchService.GetTreasuryAsync();
        return Ok(treasury);
    }

    [HttpPost("disbursements")]
    public async Task<ActionResult<BranchDisbursementResponse>> CreateDisbursement(
        [FromBody] CreateBranchDisbursementRequest request)
    {
        try
        {
            var disbursement = await _branchService.CreateDisbursementAsync(request);
            return Ok(disbursement);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("loans")]
    public async Task<ActionResult<IReadOnlyList<LoanAdminResponse>>> GetLoans(
        [FromQuery] Common.Enums.LoanStatus? status)
    {
        var loans = await _loanService.GetLoansForAdminAsync(status);
        return Ok(loans);
    }

    [HttpPost("loans/{id:guid}/approve")]
    public async Task<ActionResult<LoanAdminResponse>> ApproveLoan(Guid id)
    {
        try
        {
            var loan = await _loanService.ApproveLoanAsync(id);
            return Ok(loan);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("loans/{id:guid}/reject")]
    public async Task<ActionResult<LoanAdminResponse>> RejectLoan(
        Guid id,
        [FromBody] RejectLoanRequest request)
    {
        try
        {
            var loan = await _loanService.RejectLoanAsync(id, request);
            return Ok(loan);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
