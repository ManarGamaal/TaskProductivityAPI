using TaskProductivity.Application.Common;
using TaskProductivity.Application.DTOs.Tasks;
using TaskProductivity.Application.Interfaces;
using TaskProductivity.Application.Mappings;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Exceptions;
using TaskProductivity.Domain.Interfaces;

namespace TaskProductivity.Application.Services;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<TaskDto>> GetFilteredAsync(
        TaskFilterQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _taskRepository.GetFilteredPagedAsync(
            _currentUserService.UserId,
            query.Status,
            query.Priority,
            query.ProjectId,
            query.DueFromUtc,
            query.DueToUtc,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = items.Select(t => t.ToDto()).ToList();
        return new PagedResult<TaskDto>(dtos, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<TaskDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        return task.ToDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (dto.ProjectId.HasValue)
        {
            var projectExists = await _projectRepository.ExistsForUserAsync(dto.ProjectId.Value, userId, cancellationToken);
            if (!projectExists)
                throw new NotFoundException(nameof(Project), dto.ProjectId.Value);
        }

        var task = new TaskItem(
            dto.Title,
            dto.Description,
            dto.DueDateUtc,
            dto.Priority,
            dto.ProjectId,
            userId);

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _taskRepository.GetByIdAsync(task.Id, userId, cancellationToken);
        return (created ?? task).ToDto();
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var task = await _taskRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        if (dto.ProjectId.HasValue)
        {
            var projectExists = await _projectRepository.ExistsForUserAsync(dto.ProjectId.Value, userId, cancellationToken);
            if (!projectExists)
                throw new NotFoundException(nameof(Project), dto.ProjectId.Value);
        }

        task.UpdateDetails(dto.Title, dto.Description, dto.DueDateUtc, dto.Priority, dto.ProjectId);

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _taskRepository.GetByIdAsync(task.Id, userId, cancellationToken);
        return (updated ?? task).ToDto();
    }

    public async Task<TaskDto> ChangeStatusAsync(
        Guid id,
        ChangeTaskStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var task = await _taskRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        task.ChangeStatus(dto.Status);

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        _taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
