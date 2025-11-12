namespace CommunityCar.Api.Models;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object>? Data { get; set; }

    protected Result(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, message);
    }

    public static Result Failure(string message)
    {
        return new Result(false, message);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        var result = new Result(false);
        result.Errors.AddRange(errors);
        return result;
    }

    public static Result Failure(string message, IEnumerable<string> errors)
    {
        var result = new Result(false, message);
        result.Errors.AddRange(errors);
        return result;
    }

    public Result WithData(string key, object value)
    {
        Data ??= new Dictionary<string, object>();
        Data[key] = value;
        return this;
    }

    public Result WithData(Dictionary<string, object> data)
    {
        Data ??= new Dictionary<string, object>();
        foreach (var kvp in data)
        {
            Data[kvp.Key] = kvp.Value;
        }
        return this;
    }
}

public class Result<T> : Result
{
    public T? Value { get; set; }

    protected Result(T? value, bool isSuccess, string? message = null)
        : base(isSuccess, message)
    {
        Value = value;
    }

    public new static Result<T> Success(T value, string? message = null)
    {
        return new Result<T>(value, true, message);
    }

    public new static Result<T> Failure(string message)
    {
        return new Result<T>(default, false, message);
    }

    public new static Result<T> Failure(IEnumerable<string> errors)
    {
        var result = new Result<T>(default, false);
        result.Errors.AddRange(errors);
        return result;
    }

    public new static Result<T> Failure(string message, IEnumerable<string> errors)
    {
        var result = new Result<T>(default, false, message);
        result.Errors.AddRange(errors);
        return result;
    }

    public Result<T> WithValue(T value)
    {
        Value = value;
        return this;
    }
}

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value, string? message = null)
    {
        return Result<T>.Success(value, message);
    }

    public static Result ToResult(this bool condition, string successMessage, string failureMessage)
    {
        return condition ? Result.Success(successMessage) : Result.Failure(failureMessage);
    }

    public static async Task<Result<T>> OnSuccess<T>(this Result<T> result, Func<T, Task<Result<T>>> func)
    {
        if (!result.IsSuccess)
            return result;

        return await func(result.Value!);
    }

    public static async Task<Result<U>> OnSuccess<T, U>(this Result<T> result, Func<T, Task<Result<U>>> func)
    {
        if (!result.IsSuccess)
            return Result<U>.Failure(result.Message ?? "Operation failed", result.Errors);

        return await func(result.Value!);
    }

    public static Result<T> OnSuccess<T>(this Result<T> result, Func<T, Result<T>> func)
    {
        if (!result.IsSuccess)
            return result;

        return func(result.Value!);
    }

    public static Result<U> OnSuccess<T, U>(this Result<T> result, Func<T, Result<U>> func)
    {
        if (!result.IsSuccess)
            return Result<U>.Failure(result.Message ?? "Operation failed", result.Errors);

        return func(result.Value!);
    }

    public static async Task<Result<T>> OnFailure<T>(this Result<T> result, Func<Task> func)
    {
        if (!result.IsSuccess)
            await func();

        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action action)
    {
        if (!result.IsSuccess)
            action();

        return result;
    }

    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
    {
        if (!result.IsSuccess)
            return result;

        if (!predicate(result.Value!))
            return Result<T>.Failure(errorMessage);

        return result;
    }

    public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T, Task<bool>> predicate, string errorMessage)
    {
        if (!result.IsSuccess)
            return result;

        if (!await predicate(result.Value!))
            return Result<T>.Failure(errorMessage);

        return result;
    }

    public static T GetValueOrThrow<T>(this Result<T> result, string? message = null)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException(message ?? result.Message ?? "Operation failed");

        return result.Value!;
    }

    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default!)
    {
        return result.IsSuccess ? result.Value! : defaultValue;
    }

    public static bool TryGetValue<T>(this Result<T> result, out T value)
    {
        value = result.IsSuccess ? result.Value! : default!;
        return result.IsSuccess;
    }
}

public class ValidationResult : Result
{
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();

    protected ValidationResult(bool isSuccess, string? message = null)
        : base(isSuccess, message)
    {
    }

    public static ValidationResult Success(string? message = null)
    {
        return new ValidationResult(true, message);
    }

    public static ValidationResult Failure(string message)
    {
        return new ValidationResult(false, message);
    }

    public static ValidationResult Failure(Dictionary<string, List<string>> validationErrors)
    {
        var result = new ValidationResult(false, "Validation failed");
        result.ValidationErrors = validationErrors;
        result.Errors.AddRange(validationErrors.SelectMany(x => x.Value));
        return result;
    }

    public ValidationResult AddError(string propertyName, string errorMessage)
    {
        if (!ValidationErrors.ContainsKey(propertyName))
            ValidationErrors[propertyName] = new List<string>();

        ValidationErrors[propertyName].Add(errorMessage);
        Errors.Add(errorMessage);
        return this;
    }

    public bool HasErrorsForProperty(string propertyName)
    {
        return ValidationErrors.ContainsKey(propertyName) && ValidationErrors[propertyName].Any();
    }

    public List<string> GetErrorsForProperty(string propertyName)
    {
        return ValidationErrors.TryGetValue(propertyName, out var errors) ? errors : new List<string>();
    }
}

public class OperationResult : Result
{
    public string? OperationId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    protected OperationResult(bool isSuccess, string? message = null)
        : base(isSuccess, message)
    {
    }

    public static OperationResult Success(string? message = null, string? operationId = null)
    {
        return new OperationResult(true, message) { OperationId = operationId };
    }

    public static OperationResult Failure(string message, string? operationId = null)
    {
        return new OperationResult(false, message) { OperationId = operationId };
    }

    public OperationResult MarkCompleted()
    {
        CompletedAt = DateTime.UtcNow;
        return this;
    }
}

public class PagedResult<T> : Result<List<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    protected PagedResult(List<T> items, int page, int pageSize, int totalCount, bool isSuccess, string? message = null)
        : base(items, isSuccess, message)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Success(List<T> items, int page, int pageSize, int totalCount, string? message = null)
    {
        return new PagedResult<T>(items, page, pageSize, totalCount, true, message);
    }

    public static PagedResult<T> Failure(string message)
    {
        return new PagedResult<T>(new List<T>(), 1, 0, 0, false, message);
    }
}