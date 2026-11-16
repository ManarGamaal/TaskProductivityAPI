using TaskProductivity.Application.DTOs.TimeLogs;

namespace TaskProductivity.Application.Interfaces;

public interface ITimeTrackingService
{
    Task<TimeLogDto?> GetActiveTimerAsync(CancellationToken cancellationToken = default);
    Task<TimeLogDto> StartTimerAsync(StartTimerDto dto, CancellationToken cancellationToken = default);
    Task<TimeLogDto> StopTimerAsync(StopTimerDto dto, CancellationToken cancellationToken = default);
    Task<TimeLogDto> CreateManualLogAsync(CreateManualTimeLogDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeLogDto>> GetLogsByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task DeleteLogAsync(Guid id, CancellationToken cancellationToken = default);
}
