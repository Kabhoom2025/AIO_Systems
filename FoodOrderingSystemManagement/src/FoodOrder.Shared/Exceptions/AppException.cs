namespace FoodOrder.Shared.Exceptions;

/// <summary>
/// Represents a known, expected business-rule or validation failure.
/// The ExceptionMiddleware converts this into the correct HTTP status code automatically.
/// Use this instead of returning error DTOs from services — keeps service logic clean.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
