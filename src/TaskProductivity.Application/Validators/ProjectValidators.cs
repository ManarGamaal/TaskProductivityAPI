using FluentValidation;
using TaskProductivity.Application.DTOs.Projects;

namespace TaskProductivity.Application.Validators;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(150).WithMessage("Project title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ColorCode)
            .NotEmpty().WithMessage("Color code is required.")
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color code must be a valid hex code (e.g., #3B82F6).");
    }
}

public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(150).WithMessage("Project title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ColorCode)
            .NotEmpty().WithMessage("Color code is required.")
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color code must be a valid hex code (e.g., #3B82F6).");
    }
}
