using TaskProductivity.Application.DTOs.Analytics;
using TaskProductivity.Application.Interfaces;
using TaskProductivity.Domain.Interfaces;
using DomainTaskStatus = TaskProductivity.Domain.Enums.TaskStatus;

namespace TaskProductivity.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeLogRepository _timeLogRepository;
    private readonly ICurrentUserService _currentUserService;

    public AnalyticsService(
        ITaskRepository taskRepository,
        ITimeLogRepository timeLogRepository,
        ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _timeLogRepository = timeLogRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DailySummaryDto> GetDailySummaryAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var dayStartUtc = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEndUtc = dayStartUtc.AddDays(1);

        var tasks = await _taskRepository.GetTasksForAnalyticsAsync(userId, dayStartUtc, dayEndUtc, cancellationToken);
        var timeLogs = await _timeLogRepository.GetByDateRangeAsync(userId, dayStartUtc, dayEndUtc, cancellationToken);

        // Total tasks completed on this date (by CompletedAtUtc)
        var completedTasksOnDate = tasks
            .Where(t => t.Status == DomainTaskStatus.Completed
                        && t.CompletedAtUtc.HasValue
                        && t.CompletedAtUtc.Value >= dayStartUtc
                        && t.CompletedAtUtc.Value < dayEndUtc)
            .ToList();

        // Due tasks on this date (by DueDateUtc)
        var dueTasksOnDate = tasks
            .Where(t => t.DueDateUtc.HasValue
                        && t.DueDateUtc.Value >= dayStartUtc
                        && t.DueDateUtc.Value < dayEndUtc)
            .ToList();

        var completedDueTasksCount = dueTasksOnDate.Count(t => t.Status == DomainTaskStatus.Completed);
        var completionRate = dueTasksOnDate.Count == 0
            ? 0.0
            : Math.Round((double)completedDueTasksCount / dueTasksOnDate.Count * 100.0, 2);

        // Time logs started on this date (by StartTimeUtc)
        var logsStartedOnDate = timeLogs
            .Where(l => l.StartTimeUtc >= dayStartUtc && l.StartTimeUtc < dayEndUtc)
            .ToList();

        double GetEffectiveMinutes(Domain.Entities.TimeLog log) =>
            log.IsRunning
                ? Math.Round(Math.Max(0, (DateTime.UtcNow - log.StartTimeUtc).TotalMinutes), 2)
                : log.DurationInMinutes;

        var totalMinutes = Math.Round(logsStartedOnDate.Sum(GetEffectiveMinutes), 2);
        var totalHours = Math.Round(totalMinutes / 60.0, 2);

        // Group breakdown by Project
        var projectGroups = new Dictionary<Guid, (Guid? ProjectId, string Title, string Color, double Minutes, int CompletedCount)>();
        var unassignedKey = Guid.Empty;

        foreach (var log in logsStartedOnDate)
        {
            var projId = log.TaskItem?.ProjectId;
            var key = projId ?? unassignedKey;
            var title = log.TaskItem?.Project?.Title ?? "Unassigned";
            var color = log.TaskItem?.Project?.ColorCode ?? "#6B7280";

            if (!projectGroups.TryGetValue(key, out var entry))
            {
                entry = (projId, title, color, 0, 0);
            }

            entry.Minutes += GetEffectiveMinutes(log);
            projectGroups[key] = entry;
        }

        foreach (var task in completedTasksOnDate)
        {
            var projId = task.ProjectId;
            var key = projId ?? unassignedKey;
            var title = task.Project?.Title ?? "Unassigned";
            var color = task.Project?.ColorCode ?? "#6B7280";

            if (!projectGroups.TryGetValue(key, out var entry))
            {
                entry = (projId, title, color, 0, 0);
            }

            entry.CompletedCount += 1;
            projectGroups[key] = entry;
        }

        var breakdown = projectGroups.Values
            .Select(g => new ProjectTimeBreakdownDto(
                g.ProjectId,
                g.Title,
                g.Color,
                Math.Round(g.Minutes, 2),
                Math.Round(g.Minutes / 60.0, 2),
                g.CompletedCount))
            .OrderByDescending(b => b.TotalMinutes)
            .ThenByDescending(b => b.TasksCompletedCount)
            .ToList();

        return new DailySummaryDto(
            date,
            completedTasksOnDate.Count,
            dueTasksOnDate.Count,
            completedDueTasksCount,
            completionRate,
            totalMinutes,
            totalHours,
            breakdown);
    }

    public async Task<WeeklySummaryDto> GetWeeklySummaryAsync(
        DateOnly startDate,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var weekStartUtc = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEndUtc = weekStartUtc.AddDays(7);
        var endDate = startDate.AddDays(6);

        var tasks = await _taskRepository.GetTasksForAnalyticsAsync(userId, weekStartUtc, weekEndUtc, cancellationToken);
        var timeLogs = await _timeLogRepository.GetByDateRangeAsync(userId, weekStartUtc, weekEndUtc, cancellationToken);

        double GetEffectiveMinutes(Domain.Entities.TimeLog log) =>
            log.IsRunning
                ? Math.Round(Math.Max(0, (DateTime.UtcNow - log.StartTimeUtc).TotalMinutes), 2)
                : log.DurationInMinutes;

        var dailyPoints = new List<DailyProductivityPointDto>(7);

        for (int i = 0; i < 7; i++)
        {
            var currentDate = startDate.AddDays(i);
            var dayStart = weekStartUtc.AddDays(i);
            var dayEnd = dayStart.AddDays(1);

            var completedCount = tasks.Count(t =>
                t.Status == DomainTaskStatus.Completed &&
                t.CompletedAtUtc.HasValue &&
                t.CompletedAtUtc.Value >= dayStart &&
                t.CompletedAtUtc.Value < dayEnd);

            var minutesTracked = Math.Round(
                timeLogs
                    .Where(l => l.StartTimeUtc >= dayStart && l.StartTimeUtc < dayEnd)
                    .Sum(GetEffectiveMinutes),
                2);

            var hoursTracked = Math.Round(minutesTracked / 60.0, 2);

            dailyPoints.Add(new DailyProductivityPointDto(
                currentDate,
                currentDate.DayOfWeek.ToString(),
                completedCount,
                minutesTracked,
                hoursTracked));
        }

        var totalTasksCompleted = dailyPoints.Sum(d => d.TasksCompleted);
        var totalMinutesLogged = Math.Round(dailyPoints.Sum(d => d.MinutesTracked), 2);
        var totalHoursLogged = Math.Round(totalMinutesLogged / 60.0, 2);

        var mostProductiveDay = dailyPoints
            .Where(d => d.TasksCompleted > 0 || d.MinutesTracked > 0)
            .OrderByDescending(d => d.TasksCompleted)
            .ThenByDescending(d => d.MinutesTracked)
            .FirstOrDefault();

        return new WeeklySummaryDto(
            startDate,
            endDate,
            totalTasksCompleted,
            totalMinutesLogged,
            totalHoursLogged,
            mostProductiveDay,
            dailyPoints);
    }
}
