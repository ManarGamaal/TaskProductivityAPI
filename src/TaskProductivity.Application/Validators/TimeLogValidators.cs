using FluentValidation;
using TaskProductivity.Application.DTOs.TimeLogs;

namespace TaskProductivity.Application.Validators;

public sealed class StartTimerValidator : AbstractValidator<StartTimerDto>
{
    public StartTimerValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty().WithMessage("TaskItemId is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
    }
}

public sealed class StopTimerValidator : AbstractValidator<StopTimerDto>
{
    public StopTimerValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
    }
}

public sealed class CreateManualTimeLogValidator : AbstractValidator<CreateManualTimeLogDto>
{
    public CreateManualTimeLogValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty().WithMessage("TaskItemId is required.");

        RuleFor(x => x.EndTimeUtc)
            .GreaterThan(x => x.StartTimeUtc)
            .WithMessage("EndTimeUtc must be after StartTimeUtc.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
    }
}
