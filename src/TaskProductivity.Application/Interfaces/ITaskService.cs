using TaskProductivity.Application.Common;
using TaskProductivity.Application.DTOs.Tasks;

namespace TaskProductivity.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetFilteredAsync(TaskFilterQueryDto query, CancellationToken cancellationToken = default);
    Task<TaskDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDto> ChangeStatusAsync(Guid id, ChangeTaskStatusDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
