using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskProductivity.Application.DTOs.TimeLogs;
using TaskProductivity.Application.Interfaces;

namespace TaskProductivity.API.Controllers;

[ApiController]
[Route("api/time-logs")]
[Authorize]
public sealed class TimeLogsController : ControllerBase
{
    private readonly ITimeTrackingService _timeTrackingService;

    public TimeLogsController(ITimeTrackingService timeTrackingService)
    {
        _timeTrackingService = timeTrackingService;
    }

    /// <summary>
    /// Retrieves the currently running live timer for the authenticated user, if any.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(TimeLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<TimeLogDto>> GetActiveTimer(CancellationToken cancellationToken)
    {
        var active = await _timeTrackingService.GetActiveTimerAsync(cancellationToken);
        if (active is null)
            return NoContent();

        return Ok(active);
    }

    /// <summary>
    /// Starts a live timer on a specific task. Fails if another timer is already running.
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(TimeLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimeLogDto>> StartTimer(
        [FromBody] StartTimerDto dto,
        CancellationToken cancellationToken)
    {
        var started = await _timeTrackingService.StartTimerAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, started);
    }

    /// <summary>
    /// Stops the currently running timer and calculates total duration in minutes.
    /// </summary>
    [HttpPost("stop")]
    [ProducesResponseType(typeof(TimeLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TimeLogDto>> StopTimer(
        [FromBody] StopTimerDto? dto,
        CancellationToken cancellationToken)
    {
        var stopped = await _timeTrackingService.StopTimerAsync(dto ?? new StopTimerDto(), cancellationToken);
        return Ok(stopped);
    }

    /// <summary>
    /// Logs a completed manual time entry on a task, validating against overlapping intervals.
    /// </summary>
    [HttpPost("manual")]
    [ProducesResponseType(typeof(TimeLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimeLogDto>> CreateManualLog(
        [FromBody] CreateManualTimeLogDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _timeTrackingService.CreateManualLogAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Lists all time logs recorded for a specific task.
    /// </summary>
    [HttpGet("task/{taskId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TimeLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TimeLogDto>>> GetByTask(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var logs = await _timeTrackingService.GetLogsByTaskAsync(taskId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Deletes a specific time log entry.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _timeTrackingService.DeleteLogAsync(id, cancellationToken);
        return NoContent();
    }
}
