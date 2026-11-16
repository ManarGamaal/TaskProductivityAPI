using TaskProductivity.Application.DTOs.Analytics;

namespace TaskProductivity.Application.Interfaces;

public interface IAnalyticsService
{
    Task<DailySummaryDto> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<WeeklySummaryDto> GetWeeklySummaryAsync(DateOnly startDate, CancellationToken cancellationToken = default);
}
