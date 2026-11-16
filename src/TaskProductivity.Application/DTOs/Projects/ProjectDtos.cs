namespace TaskProductivity.Application.DTOs.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Title,
    string? Description,
    string ColorCode,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateProjectDto(
    string Title,
    string? Description,
    string ColorCode = "#3B82F6");

public sealed record UpdateProjectDto(
    string Title,
    string? Description,
    string ColorCode);
