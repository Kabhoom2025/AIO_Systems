namespace AIO_Systems.Shared;

/// <summary>Lightweight exception carrying an HTTP status code, caught by the exception-handling middleware in Program.cs.</summary>
public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
