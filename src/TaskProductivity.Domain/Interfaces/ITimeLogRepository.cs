using TaskProductivity.Domain.Entities;

namespace TaskProductivity.Domain.Interfaces;

public interface ITimeLogRepository
{
    Task<TimeLog?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<TimeLog?> GetActiveTimerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        Guid? excludeTimeLogId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeLog>> GetByTaskAsync(Guid taskItemId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeLog>> GetByDateRangeAsync(
        Guid userId,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        CancellationToken cancellationToken = default);
    Task AddAsync(TimeLog timeLog, CancellationToken cancellationToken = default);
    void Update(TimeLog timeLog);
    void Remove(TimeLog timeLog);
}
