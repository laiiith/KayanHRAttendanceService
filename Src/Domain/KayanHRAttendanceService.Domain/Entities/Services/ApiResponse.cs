namespace KayanHRAttendanceService.Domain.Entities.Services;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; private set; } = new();

    public static ApiResponse<T> Success(T data, Dictionary<string, string> headers, int statusCode = 200) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = statusCode,
        Headers = headers
    };

    public static ApiResponse<T> Fail(string errorMessage, int statusCode = 500) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        StatusCode = statusCode
    };
}