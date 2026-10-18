using System.ComponentModel.DataAnnotations;
using Nstech.OrderService.Domain;

namespace Nstech.OrderService.Application.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = Messages.InvalidCredentials)]
    [EmailAddress(ErrorMessage = Messages.InvalidCredentials)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = Messages.InvalidCredentials)]
    public string Password { get; set; } = string.Empty;
}

public record TokenResponse(string AccessToken, DateTime ExpiresAt);
