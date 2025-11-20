namespace VerticalSliceApp.Api.Common;

/// <summary>
/// Standard API response wrapper for responses with data
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }

    private ApiResponse(bool success, T? data, string? message, List<string>? errors)
    {
        Success = success;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new(true, data, message, null);

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new(false, default, message, errors);

    public static ApiResponse<T> Fail(List<string> errors)
        => new(false, default, "Validation failed", errors);
}

/// <summary>
/// Standard API response wrapper for responses without data
/// </summary>
public record ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }

    private ApiResponse(bool success, string? message, List<string>? errors)
    {
        Success = success;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse Ok(string? message = null)
        => new(true, message, null);

    public static ApiResponse Fail(string message, List<string>? errors = null)
        => new(false, message, errors);

    public static ApiResponse Fail(List<string> errors)
        => new(false, "Operation failed", errors);
}
