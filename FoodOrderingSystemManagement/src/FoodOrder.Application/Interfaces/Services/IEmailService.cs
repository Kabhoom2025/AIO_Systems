namespace FoodOrder.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
}
