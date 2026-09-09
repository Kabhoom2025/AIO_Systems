using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
