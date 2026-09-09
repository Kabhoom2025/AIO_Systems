namespace FoodOrder.Shared.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden. You do not have permission to perform this action.")
        : base(message, statusCode: 403)
    {
    }
}
