using FluentValidation;
using TaskProductivity.Application.DTOs.Tasks;

namespace TaskProductivity.Application.Validators;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority value.");
    }
}

public sealed class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority value.");
    }
}

public sealed class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusDto>
{
    public ChangeTaskStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status value.");
    }
}

public sealed class TaskFilterQueryValidator : AbstractValidator<TaskFilterQueryDto>
{
    public TaskFilterQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x)
            .Must(x => !x.DueFromUtc.HasValue || !x.DueToUtc.HasValue || x.DueFromUtc <= x.DueToUtc)
            .WithMessage("DueFromUtc cannot be later than DueToUtc.");
    }
}
