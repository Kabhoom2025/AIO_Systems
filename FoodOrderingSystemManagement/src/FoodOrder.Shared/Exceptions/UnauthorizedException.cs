namespace FoodOrder.Shared.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized. Please login to access this resource.")
        : base(message, statusCode: 401)
    {
    }
}
