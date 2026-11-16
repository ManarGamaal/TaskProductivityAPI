using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Enums;
using DomainTaskStatus = TaskProductivity.Domain.Enums.TaskStatus;

namespace TaskProductivity.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetFilteredPagedAsync(
        Guid userId,
        DomainTaskStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueFromUtc,
        DateTime? dueToUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetTasksForAnalyticsAsync(
        Guid userId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Update(TaskItem task);
    void Remove(TaskItem task);
}
