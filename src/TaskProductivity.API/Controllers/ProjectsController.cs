using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskProductivity.Application.DTOs.Projects;
using TaskProductivity.Application.Interfaces;

namespace TaskProductivity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Retrieves all projects owned by the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllAsync(cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Retrieves a specific project by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        return Ok(project);
    }

    /// <summary>
    /// Creates a new project for the authenticated user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] CreateProjectDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _projectService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing project's title, description, or color code.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Update(
        Guid id,
        [FromBody] UpdateProjectDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _projectService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an existing project.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
