using Current.Api.DTOs.Analytics;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ICurrentUserService _currentUserService;

    public AnalyticsController(IAnalyticsService analyticsService, ICurrentUserService currentUserService)
    {
        _analyticsService = analyticsService;
        _currentUserService = currentUserService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AnalyticsOverviewResponse>> GetOverview()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var overview = await _analyticsService.GetOverviewAsync(currentUserId);
        return Ok(overview);
    }

    [HttpGet("cashflow")]
    public async Task<ActionResult<CashFlowResponse>> GetCashFlow()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var cashFlow = await _analyticsService.GetCashFlowAsync(currentUserId);
        return Ok(cashFlow);
    }

    [HttpGet("networth-history")]
    public async Task<ActionResult<NetWorthHistoryResponse>> GetNetWorthHistory()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var netWorthHistory = await _analyticsService.GetNetWorthHistoryAsync(currentUserId);
        return Ok(netWorthHistory);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<CategoryBreakdownResponse>> GetCategoryBreakdown()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var categoryBreakdown = await _analyticsService.GetCategoryBreakdownAsync(currentUserId);
        return Ok(categoryBreakdown);
    }

    [HttpGet("goals")]
    public async Task<ActionResult<GoalProgressResponse>> GetGoalProgress()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var goalProgress = await _analyticsService.GetGoalProgressAsync(currentUserId);
        return Ok(goalProgress);
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryResponse>> GetMonthlySummary()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var monthlySummary = await _analyticsService.GetMonthlySummaryAsync(currentUserId);
        return Ok(monthlySummary);
    }
}
