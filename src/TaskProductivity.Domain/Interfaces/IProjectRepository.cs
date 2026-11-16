using TaskProductivity.Domain.Entities;

namespace TaskProductivity.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Update(Project project);
    void Remove(Project project);
    Task<bool> ExistsForUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
