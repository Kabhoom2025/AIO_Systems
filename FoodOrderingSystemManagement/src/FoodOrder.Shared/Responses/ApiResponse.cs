namespace FoodOrder.Shared.Responses;

/// <summary>
/// Standard envelope returned by every API endpoint.
/// Keeps the client contract consistent regardless of which module is called.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResult(string message) =>
        new() { Success = false, Message = message, Data = default };
}
