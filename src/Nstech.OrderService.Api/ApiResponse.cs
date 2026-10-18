namespace Nstech.OrderService.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Body { get; set; }

    public static ApiResponse<T> Ok(T body, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Body = body
        };
    }

    public static ApiResponse<T> Fail(string message, int statusCode)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
