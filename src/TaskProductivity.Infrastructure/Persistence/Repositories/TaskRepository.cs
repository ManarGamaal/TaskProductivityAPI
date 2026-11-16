using Microsoft.EntityFrameworkCore;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Enums;
using TaskProductivity.Domain.Interfaces;
using DomainTaskStatus = TaskProductivity.Domain.Enums.TaskStatus;

namespace TaskProductivity.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetFilteredPagedAsync(
        Guid userId,
        DomainTaskStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueFromUtc,
        DateTime? dueToUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Where(t => t.UserId == userId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (dueFromUtc.HasValue)
        {
            var from = DateTime.SpecifyKind(dueFromUtc.Value, DateTimeKind.Utc);
            query = query.Where(t => t.DueDateUtc.HasValue && t.DueDateUtc.Value >= from);
        }

        if (dueToUtc.HasValue)
        {
            var to = DateTime.SpecifyKind(dueToUtc.Value, DateTimeKind.Utc);
            query = query.Where(t => t.DueDateUtc.HasValue && t.DueDateUtc.Value <= to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDateUtc ?? DateTime.MaxValue)
            .ThenByDescending(t => t.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksForAnalyticsAsync(
        Guid userId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Where(t => t.UserId == userId &&
                        ((t.CompletedAtUtc.HasValue && t.CompletedAtUtc.Value >= rangeStartUtc && t.CompletedAtUtc.Value < rangeEndUtc) ||
                         (t.DueDateUtc.HasValue && t.DueDateUtc.Value >= rangeStartUtc && t.DueDateUtc.Value < rangeEndUtc)))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
    }

    public void Update(TaskItem task)
    {
        _dbContext.Tasks.Update(task);
    }

    public void Remove(TaskItem task)
    {
        _dbContext.Tasks.Remove(task);
    }
}
