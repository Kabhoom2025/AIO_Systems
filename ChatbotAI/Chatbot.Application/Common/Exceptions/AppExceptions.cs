namespace Chatbot.Application.Common.Exceptions;

public class NotFoundException(string entity, object key)
    : Exception($"{entity} with id '{key}' was not found.")
{
    public string ErrorCode => "NOT_FOUND";
}

public class ValidationAppException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
    public string ErrorCode => "VALIDATION_ERROR";
}

public class UnauthorizedAppException(string message = "Invalid credentials.")
    : Exception(message)
{
    public string ErrorCode => "UNAUTHORIZED";
}

public class ConflictAppException(string message)
    : Exception(message)
{
    public string ErrorCode => "CONFLICT";
}

public class AiServiceException(string message, Exception? inner = null)
    : Exception(message, inner)
{
    public string ErrorCode => "AI_SERVICE_ERROR";
}
