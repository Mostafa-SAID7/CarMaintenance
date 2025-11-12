namespace CommunityCar.Domain.Exceptions;

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BusinessRuleViolationException(string message, string ruleName) : base(message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleViolationException(string message, string ruleName, Exception innerException)
        : base(message, innerException)
    {
        RuleName = ruleName;
    }

    public string? RuleName { get; }
}
