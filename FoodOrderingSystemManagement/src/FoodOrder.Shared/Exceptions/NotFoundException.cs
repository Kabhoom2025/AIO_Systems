namespace FoodOrder.Shared.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist.
/// Middleware maps this to HTTP 404.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.", statusCode: 404)
    {
    }

    public NotFoundException(string message)
        : base(message, statusCode: 404)
    {
    }
}
