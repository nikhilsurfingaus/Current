using Current.Api.DTOs.Goals;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("goals")]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;
    private readonly ICurrentUserService _currentUserService;

    public GoalsController(IGoalService goalService, ICurrentUserService currentUserService)
    {
        _goalService = goalService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GoalResponse>>> GetAll()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var goals = await _goalService.GetAllGoalsAsync(currentUserId);
        return Ok(goals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> GetById(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var goal = await _goalService.GetGoalByIdAsync(id, currentUserId);

        if (goal is null)
        {
            return NotFound();
        }

        return Ok(goal);
    }

    [HttpPost]
    public async Task<ActionResult<GoalResponse>> Create([FromBody] CreateGoalRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var goal = await _goalService.CreateGoalAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> Update(Guid id, [FromBody] UpdateGoalRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var goal = await _goalService.UpdateGoalAsync(id, request, currentUserId);

            if (goal is null)
            {
                return NotFound();
            }

            return Ok(goal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> Cancel(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var goal = await _goalService.CancelGoalAsync(id, currentUserId);

        if (goal is null)
        {
            return NotFound();
        }

        return Ok(goal);
    }

    [HttpPost("{id:guid}/contribute")]
    public async Task<ActionResult<GoalResponse>> Contribute(Guid id, [FromBody] ContributeGoalRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var goal = await _goalService.ContributeToGoalAsync(id, request, currentUserId);
            return Ok(goal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<GoalResponse>> Withdraw(Guid id, [FromBody] WithdrawGoalRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var goal = await _goalService.WithdrawFromGoalAsync(id, request, currentUserId);
            return Ok(goal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<GoalContributionResponse>>> GetHistory(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var goal = await _goalService.GetGoalByIdAsync(id, currentUserId);

        if (goal is null)
        {
            return NotFound();
        }

        var history = await _goalService.GetContributionHistoryAsync(id, currentUserId);
        return Ok(history);
    }
}
