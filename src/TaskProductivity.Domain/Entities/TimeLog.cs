using TaskProductivity.Domain.Common;
using TaskProductivity.Domain.Exceptions;

namespace TaskProductivity.Domain.Entities;

public class TimeLog : BaseEntity
{
    public Guid TaskItemId { get; private set; }
    public TaskItem TaskItem { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime StartTimeUtc { get; private set; }
    public DateTime? EndTimeUtc { get; private set; }
    public double DurationInMinutes { get; private set; }
    public string? Notes { get; private set; }

    public bool IsRunning => !EndTimeUtc.HasValue;

    private TimeLog() { } // Required by EF Core

    /// <summary>
    /// Starts a new live timer on a task.
    /// </summary>
    public static TimeLog StartTimer(Guid taskItemId, Guid userId, string? notes = null)
    {
        if (taskItemId == Guid.Empty)
            throw new BusinessRuleViolationException("TaskItemId is required.");
        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("UserId is required.");

        return new TimeLog
        {
            TaskItemId = taskItemId,
            UserId = userId,
            StartTimeUtc = DateTime.UtcNow,
            EndTimeUtc = null,
            DurationInMinutes = 0,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    /// <summary>
    /// Creates a completed manual time log entry.
    /// </summary>
    public static TimeLog CreateManualLog(
        Guid taskItemId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string? notes = null)
    {
        if (taskItemId == Guid.Empty)
            throw new BusinessRuleViolationException("TaskItemId is required.");
        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("UserId is required.");

        var normalizedStart = DateTime.SpecifyKind(startTimeUtc, DateTimeKind.Utc);
        var normalizedEnd = DateTime.SpecifyKind(endTimeUtc, DateTimeKind.Utc);

        if (normalizedEnd <= normalizedStart)
            throw new BusinessRuleViolationException("EndTimeUtc must be strictly greater than StartTimeUtc.");

        var duration = Math.Round((normalizedEnd - normalizedStart).TotalMinutes, 2);

        return new TimeLog
        {
            TaskItemId = taskItemId,
            UserId = userId,
            StartTimeUtc = normalizedStart,
            EndTimeUtc = normalizedEnd,
            DurationInMinutes = duration,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    public void StopTimer(string? notes = null)
    {
        if (!IsRunning)
            throw new BusinessRuleViolationException("This timer has already been stopped.");

        var now = DateTime.UtcNow;
        EndTimeUtc = now < StartTimeUtc ? StartTimeUtc : now;
        DurationInMinutes = Math.Round((EndTimeUtc.Value - StartTimeUtc).TotalMinutes, 2);

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = notes.Trim();
        }

        MarkUpdated();
    }
}
