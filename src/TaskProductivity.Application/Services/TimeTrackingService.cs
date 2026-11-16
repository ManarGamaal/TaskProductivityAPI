using TaskProductivity.Application.DTOs.TimeLogs;
using TaskProductivity.Application.Interfaces;
using TaskProductivity.Application.Mappings;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Exceptions;
using TaskProductivity.Domain.Interfaces;

namespace TaskProductivity.Application.Services;

public sealed class TimeTrackingService : ITimeTrackingService
{
    private readonly ITimeLogRepository _timeLogRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TimeTrackingService(
        ITimeLogRepository timeLogRepository,
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _timeLogRepository = timeLogRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<TimeLogDto?> GetActiveTimerAsync(CancellationToken cancellationToken = default)
    {
        var activeTimer = await _timeLogRepository.GetActiveTimerAsync(_currentUserService.UserId, cancellationToken);
        return activeTimer?.ToDto();
    }

    public async Task<TimeLogDto> StartTimerAsync(StartTimerDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var task = await _taskRepository.GetByIdAsync(dto.TaskItemId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), dto.TaskItemId);

        var activeTimer = await _timeLogRepository.GetActiveTimerAsync(userId, cancellationToken);
        if (activeTimer is not null)
        {
            throw new BusinessRuleViolationException(
                $"You already have an active timer running on task '{activeTimer.TaskItem?.Title ?? activeTimer.TaskItemId.ToString()}'. Stop it before starting a new one.");
        }

        var now = DateTime.UtcNow;
        var hasOverlap = await _timeLogRepository.HasOverlapAsync(userId, now, now.AddSeconds(1), null, cancellationToken);
        if (hasOverlap)
        {
            throw new BusinessRuleViolationException("The current timestamp overlaps with an existing logged time interval.");
        }

        var timeLog = TimeLog.StartTimer(task.Id, userId, dto.Notes);

        await _timeLogRepository.AddAsync(timeLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _timeLogRepository.GetByIdAsync(timeLog.Id, userId, cancellationToken);
        return (created ?? timeLog).ToDto();
    }

    public async Task<TimeLogDto> StopTimerAsync(StopTimerDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var activeTimer = await _timeLogRepository.GetActiveTimerAsync(userId, cancellationToken)
            ?? throw new BusinessRuleViolationException("No active timer is currently running.");

        activeTimer.StopTimer(dto.Notes);

        var hasOverlap = await _timeLogRepository.HasOverlapAsync(
            userId,
            activeTimer.StartTimeUtc,
            activeTimer.EndTimeUtc!.Value,
            activeTimer.Id,
            cancellationToken);

        if (hasOverlap)
        {
            throw new BusinessRuleViolationException(
                "Stopping this timer at the current time overlaps with an existing manual time entry.");
        }

        _timeLogRepository.Update(activeTimer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return activeTimer.ToDto();
    }

    public async Task<TimeLogDto> CreateManualLogAsync(
        CreateManualTimeLogDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var task = await _taskRepository.GetByIdAsync(dto.TaskItemId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), dto.TaskItemId);

        var startUtc = DateTime.SpecifyKind(dto.StartTimeUtc, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(dto.EndTimeUtc, DateTimeKind.Utc);

        if (endUtc <= startUtc)
            throw new BusinessRuleViolationException("EndTimeUtc must be strictly greater than StartTimeUtc.");

        var activeTimer = await _timeLogRepository.GetActiveTimerAsync(userId, cancellationToken);
        if (activeTimer is not null && endUtc > activeTimer.StartTimeUtc)
        {
            throw new BusinessRuleViolationException(
                "Manual time log interval cannot overlap with your currently running timer.");
        }

        var hasOverlap = await _timeLogRepository.HasOverlapAsync(userId, startUtc, endUtc, null, cancellationToken);
        if (hasOverlap)
        {
            throw new BusinessRuleViolationException(
                "The specified time interval overlaps with an existing time log.");
        }

        var timeLog = TimeLog.CreateManualLog(task.Id, userId, startUtc, endUtc, dto.Notes);

        await _timeLogRepository.AddAsync(timeLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _timeLogRepository.GetByIdAsync(timeLog.Id, userId, cancellationToken);
        return (created ?? timeLog).ToDto();
    }

    public async Task<IReadOnlyList<TimeLogDto>> GetLogsByTaskAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var task = await _taskRepository.GetByIdAsync(taskId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), taskId);

        var logs = await _timeLogRepository.GetByTaskAsync(task.Id, userId, cancellationToken);
        return logs.Select(l => l.ToDto()).ToList();
    }

    public async Task DeleteLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _timeLogRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(TimeLog), id);

        _timeLogRepository.Remove(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
