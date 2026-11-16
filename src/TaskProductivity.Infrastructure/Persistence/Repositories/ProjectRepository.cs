using Microsoft.EntityFrameworkCore;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Interfaces;

namespace TaskProductivity.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public void Update(Project project)
    {
        _dbContext.Projects.Update(project);
    }

    public void Remove(Project project)
    {
        _dbContext.Projects.Remove(project);
    }

    public async Task<bool> ExistsForUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.UserId == userId, cancellationToken);
    }
}
