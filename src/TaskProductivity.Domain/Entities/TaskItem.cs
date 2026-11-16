using TaskProductivity.Domain.Common;
using TaskProductivity.Domain.Enums;
using TaskProductivity.Domain.Exceptions;
using DomainTaskStatus = TaskProductivity.Domain.Enums.TaskStatus;

namespace TaskProductivity.Domain.Entities;

public class TaskItem : BaseEntity
{
    private readonly List<TimeLog> _timeLogs = new();

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DomainTaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? ProjectId { get; private set; }
    public Project? Project { get; private set; }
    public Guid UserId { get; private set; }

    public IReadOnlyCollection<TimeLog> TimeLogs => _timeLogs.AsReadOnly();

    private TaskItem() { } // Required by EF Core

    public TaskItem(
        string title,
        string? description,
        DateTime? dueDateUtc,
        TaskPriority priority,
        Guid? projectId,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Task title cannot be empty.");

        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("UserId is required.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DueDateUtc = dueDateUtc.HasValue ? DateTime.SpecifyKind(dueDateUtc.Value, DateTimeKind.Utc) : null;
        Priority = priority;
        ProjectId = projectId;
        UserId = userId;
        Status = DomainTaskStatus.Todo;
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateTime? dueDateUtc,
        TaskPriority priority,
        Guid? projectId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Task title cannot be empty.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DueDateUtc = dueDateUtc.HasValue ? DateTime.SpecifyKind(dueDateUtc.Value, DateTimeKind.Utc) : null;
        Priority = priority;
        ProjectId = projectId;
        MarkUpdated();
    }

    public void ChangeStatus(DomainTaskStatus newStatus)
    {
        if (Status == newStatus)
            return;

        Status = newStatus;

        if (newStatus == DomainTaskStatus.Completed)
        {
            CompletedAtUtc = DateTime.UtcNow;
        }
        else
        {
            CompletedAtUtc = null;
        }

        MarkUpdated();
    }
}
