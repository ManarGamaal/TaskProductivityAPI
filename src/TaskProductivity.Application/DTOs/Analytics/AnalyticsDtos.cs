namespace TaskProductivity.Application.DTOs.Analytics;

public sealed record ProjectTimeBreakdownDto(
    Guid? ProjectId,
    string ProjectTitle,
    string ColorCode,
    double TotalMinutes,
    double TotalHours,
    int TasksCompletedCount);

public sealed record DailySummaryDto(
    DateOnly Date,
    int TotalTasksCompleted,
    int TotalDueTasksOnDate,
    int CompletedDueTasksOnDate,
    double CompletionRatePercentage,
    double TotalMinutesTracked,
    double TotalHoursTracked,
    IReadOnlyList<ProjectTimeBreakdownDto> BreakdownByProject);

public sealed record DailyProductivityPointDto(
    DateOnly Date,
    string DayOfWeek,
    int TasksCompleted,
    double MinutesTracked,
    double HoursTracked);

public sealed record WeeklySummaryDto(
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalTasksCompleted,
    double TotalMinutesLogged,
    double TotalHoursLogged,
    DailyProductivityPointDto? MostProductiveDay,
    IReadOnlyList<DailyProductivityPointDto> DailyBreakdown);
