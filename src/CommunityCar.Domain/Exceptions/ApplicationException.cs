namespace CommunityCar.Domain.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message)
    {
    }

    public ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ApplicationException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public ApplicationException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string? ErrorCode { get; }
}