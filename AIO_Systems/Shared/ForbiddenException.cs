namespace AIO_Systems.Shared;

/// <summary>403 flavor of AppException — used for license/authorization failures that are
/// distinct from "not authenticated" (401) or "not found" (404).</summary>
public class ForbiddenException(string message) : AppException(message, statusCode: 403);
