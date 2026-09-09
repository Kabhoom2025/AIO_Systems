namespace FlowSphere.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    QuotaExceeded,
    Unexpected
}

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public IDictionary<string, string[]>? ValidationErrors { get; }

    private Error(string code, string message, ErrorType type, IDictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message;
        Type = type;
        ValidationErrors = validationErrors;
    }

    public static Error NotFound(string message, string code = "NotFound") => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string message, string code = "Conflict") => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string message, string code = "Unauthorized") => new(code, message, ErrorType.Unauthorized);
    public static Error QuotaExceeded(string message, string code = "QuotaExceeded") => new(code, message, ErrorType.QuotaExceeded);
    public static Error Unexpected(string message, string code = "Unexpected") => new(code, message, ErrorType.Unexpected);

    public static Error Validation(IDictionary<string, string[]> validationErrors) =>
        new("Validation", "One or more validation errors occurred.", ErrorType.Validation, validationErrors);
}
