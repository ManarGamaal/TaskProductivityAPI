using Microsoft.EntityFrameworkCore;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Interfaces;

namespace TaskProductivity.Infrastructure.Persistence.Repositories;

public sealed class TimeLogRepository : ITimeLogRepository
{
    private readonly AppDbContext _dbContext;

    public TimeLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TimeLog?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TimeLogs
            .Include(l => l.TaskItem)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId, cancellationToken);
    }

    public async Task<TimeLog?> GetActiveTimerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TimeLogs
            .Include(l => l.TaskItem)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(l => l.UserId == userId && l.EndTimeUtc == null, cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        Guid? excludeTimeLogId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TimeLogs
            .AsNoTracking()
            .Where(l => l.UserId == userId && l.EndTimeUtc != null);

        if (excludeTimeLogId.HasValue)
        {
            query = query.Where(l => l.Id != excludeTimeLogId.Value);
        }

        // Standard interval overlap condition: ExistingStart < NewEnd && ExistingEnd > NewStart
        return await query.AnyAsync(
            l => l.StartTimeUtc < endTimeUtc && l.EndTimeUtc!.Value > startTimeUtc,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TimeLog>> GetByTaskAsync(
        Guid taskItemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TimeLogs
            .AsNoTracking()
            .Include(l => l.TaskItem)
                .ThenInclude(t => t.Project)
            .Where(l => l.TaskItemId == taskItemId && l.UserId == userId)
            .OrderByDescending(l => l.StartTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeLog>> GetByDateRangeAsync(
        Guid userId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TimeLogs
            .AsNoTracking()
            .Include(l => l.TaskItem)
                .ThenInclude(t => t.Project)
            .Where(l => l.UserId == userId &&
                        l.StartTimeUtc >= rangeStartUtc &&
                        l.StartTimeUtc < rangeEndUtc)
            .OrderBy(l => l.StartTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TimeLog timeLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.TimeLogs.AddAsync(timeLog, cancellationToken);
    }

    public void Update(TimeLog timeLog)
    {
        _dbContext.TimeLogs.Update(timeLog);
    }

    public void Remove(TimeLog timeLog)
    {
        _dbContext.TimeLogs.Remove(timeLog);
    }
}
