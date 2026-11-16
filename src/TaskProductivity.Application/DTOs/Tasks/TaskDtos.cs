using TaskProductivity.Domain.Enums;
using DomainTaskStatus = TaskProductivity.Domain.Enums.TaskStatus;

namespace TaskProductivity.Application.DTOs.Tasks;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    DateTime? CompletedAtUtc,
    DomainTaskStatus Status,
    TaskPriority Priority,
    Guid? ProjectId,
    string? ProjectTitle,
    string? ProjectColorCode,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority = TaskPriority.Medium,
    Guid? ProjectId = null);

public sealed record UpdateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority,
    Guid? ProjectId);

public sealed record ChangeTaskStatusDto(
    DomainTaskStatus Status);

public sealed record TaskFilterQueryDto(
    DomainTaskStatus? Status = null,
    TaskPriority? Priority = null,
    Guid? ProjectId = null,
    DateTime? DueFromUtc = null,
    DateTime? DueToUtc = null,
    int PageNumber = 1,
    int PageSize = 20);
