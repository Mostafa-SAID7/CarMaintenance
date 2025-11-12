namespace CommunityCar.Domain.Exceptions;

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConcurrencyException(string message, string entityName, object entityId)
        : base(message)
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public ConcurrencyException(string message, string entityName, object entityId, Exception innerException)
        : base(message, innerException)
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public string? EntityName { get; }
    public object? EntityId { get; }
}
