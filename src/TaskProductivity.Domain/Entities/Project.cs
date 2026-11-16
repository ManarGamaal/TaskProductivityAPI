using TaskProductivity.Domain.Common;
using TaskProductivity.Domain.Exceptions;

namespace TaskProductivity.Domain.Entities;

public class Project : BaseEntity
{
    private readonly List<TaskItem> _tasks = new();

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ColorCode { get; private set; } = "#3B82F6";
    public Guid UserId { get; private set; }

    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    private Project() { } // Required by EF Core

    public Project(string title, string? description, string colorCode, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Project title cannot be empty.");

        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("UserId is required.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ColorCode = ValidateAndFormatColorCode(colorCode);
        UserId = userId;
    }

    public void UpdateDetails(string title, string? description, string colorCode)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Project title cannot be empty.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ColorCode = ValidateAndFormatColorCode(colorCode);
        MarkUpdated();
    }

    private static string ValidateAndFormatColorCode(string colorCode)
    {
        if (string.IsNullOrWhiteSpace(colorCode))
            return "#3B82F6";

        var trimmed = colorCode.Trim();
        if (!trimmed.StartsWith('#') || (trimmed.Length != 4 && trimmed.Length != 7))
            throw new BusinessRuleViolationException("ColorCode must be a valid hex string (e.g., #3B82F6).");

        return trimmed.ToUpperInvariant();
    }
}
