using TaskProductivity.Application.DTOs.Projects;
using TaskProductivity.Application.DTOs.Tasks;
using TaskProductivity.Application.DTOs.TimeLogs;
using TaskProductivity.Domain.Entities;

namespace TaskProductivity.Application.Mappings;

public static class EntityMappingExtensions
{
    public static ProjectDto ToDto(this Project project) =>
        new(
            project.Id,
            project.Title,
            project.Description,
            project.ColorCode,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);

    public static TaskDto ToDto(this TaskItem task) =>
        new(
            task.Id,
            task.Title,
            task.Description,
            task.DueDateUtc,
            task.CompletedAtUtc,
            task.Status,
            task.Priority,
            task.ProjectId,
            task.Project?.Title,
            task.Project?.ColorCode,
            task.CreatedAtUtc,
            task.UpdatedAtUtc);

    public static TimeLogDto ToDto(this TimeLog log)
    {
        var liveDuration = log.IsRunning
            ? Math.Round(Math.Max(0, (DateTime.UtcNow - log.StartTimeUtc).TotalMinutes), 2)
            : log.DurationInMinutes;

        return new TimeLogDto(
            log.Id,
            log.TaskItemId,
            log.TaskItem?.Title ?? string.Empty,
            log.TaskItem?.ProjectId,
            log.TaskItem?.Project?.Title,
            log.StartTimeUtc,
            log.EndTimeUtc,
            liveDuration,
            log.IsRunning,
            log.Notes);
    }
}
