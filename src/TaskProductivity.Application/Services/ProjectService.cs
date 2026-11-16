using TaskProductivity.Application.DTOs.Projects;
using TaskProductivity.Application.Interfaces;
using TaskProductivity.Application.Mappings;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Domain.Exceptions;
using TaskProductivity.Domain.Interfaces;

namespace TaskProductivity.Application.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ProjectService(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllByUserAsync(_currentUserService.UserId, cancellationToken);
        return projects.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), id);

        return project.ToDto();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = new Project(dto.Title, dto.Description, dto.ColorCode, _currentUserService.UserId);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), id);

        project.UpdateDetails(dto.Title, dto.Description, dto.ColorCode);

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), id);

        _projectRepository.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
