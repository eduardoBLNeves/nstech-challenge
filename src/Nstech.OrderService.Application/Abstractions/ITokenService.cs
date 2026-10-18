namespace Nstech.OrderService.Application.Abstractions;

public interface ITokenService
{
    AccessToken Create(Guid userId, string email);
}

public record AccessToken(string Value, DateTime ExpiresAt);
