using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskProductivity.Application.DTOs.Analytics;
using TaskProductivity.Application.Interfaces;

namespace TaskProductivity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Retrieves daily productivity metrics including tasks completed, total time tracked, project breakdown, and due-task completion rate.
    /// </summary>
    /// <param name="date">Target date in YYYY-MM-DD format (defaults to today UTC).</param>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DailySummaryDto>> GetDailySummary(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = await _analyticsService.GetDailySummaryAsync(targetDate, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves a 7-day productivity summary starting from the specified date, including daily breakdown, most productive day, and total hours logged.
    /// </summary>
    /// <param name="startDate">Start date in YYYY-MM-DD format (defaults to 6 days ago UTC).</param>
    [HttpGet("weekly")]
    [ProducesResponseType(typeof(WeeklySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeeklySummaryDto>> GetWeeklySummary(
        [FromQuery] DateOnly? startDate,
        CancellationToken cancellationToken)
    {
        var targetStartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6));
        var summary = await _analyticsService.GetWeeklySummaryAsync(targetStartDate, cancellationToken);
        return Ok(summary);
    }
}
