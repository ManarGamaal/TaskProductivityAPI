using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskProductivity.Application.Common;
using TaskProductivity.Application.DTOs.Tasks;
using TaskProductivity.Application.Interfaces;

namespace TaskProductivity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of tasks for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetFiltered(
        [FromQuery] TaskFilterQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.GetFilteredAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific task by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing task's details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Update(
        Guid id,
        [FromBody] UpdateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Updates a task's status (Todo, InProgress, Completed) and automatically manages CompletedAtUtc.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTaskStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.ChangeStatusAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
