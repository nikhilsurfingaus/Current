using Current.Api.DTOs.Branches;
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

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
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
}
