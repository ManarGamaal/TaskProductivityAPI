namespace TaskProductivity.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.") { }
}

public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}

public sealed class UnauthorizedResourceAccessException : DomainException
{
    public UnauthorizedResourceAccessException(string message = "You do not have permission to access or modify this resource.")
        : base(message) { }
}
