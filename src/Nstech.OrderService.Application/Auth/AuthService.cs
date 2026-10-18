using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public async Task<TokenResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmail(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AppException(Messages.InvalidCredentials, 401);

        var token = _tokens.Create(user.Id, user.Email);
        return new TokenResponse(token.Value, token.ExpiresAt);
    }
}
