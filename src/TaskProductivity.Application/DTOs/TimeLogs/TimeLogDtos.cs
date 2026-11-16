namespace TaskProductivity.Application.DTOs.TimeLogs;

public sealed record TimeLogDto(
    Guid Id,
    Guid TaskItemId,
    string TaskTitle,
    Guid? ProjectId,
    string? ProjectTitle,
    DateTime StartTimeUtc,
    DateTime? EndTimeUtc,
    double DurationInMinutes,
    bool IsRunning,
    string? Notes);

public sealed record StartTimerDto(
    Guid TaskItemId,
    string? Notes = null);

public sealed record StopTimerDto(
    string? Notes = null);

public sealed record CreateManualTimeLogDto(
    Guid TaskItemId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string? Notes = null);
