using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Application.Auth;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_returns_token()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };
        var service = new AuthService(new FakeUserRepository(user), new FakePasswordHasher(true), new FakeTokenService());

        var token = await service.Login(new LoginRequest { Email = user.Email, Password = "12345678" }, CancellationToken.None);

        Assert.Equal("token", token.AccessToken);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Login_rejects_invalid_credentials(bool userExists, bool passwordMatches)
    {
        User? user = userExists
            ? new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" }
            : null;
        var service = new AuthService(new FakeUserRepository(user), new FakePasswordHasher(passwordMatches), new FakeTokenService());

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.Login(new LoginRequest { Email = "user@example.com", Password = "wrong" }, CancellationToken.None));

        Assert.Equal(401, error.StatusCode);
        Assert.Equal(Messages.InvalidCredentials, error.Message);
    }

    private class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_user);
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        private readonly bool _passwordMatches;

        public FakePasswordHasher(bool passwordMatches)
        {
            _passwordMatches = passwordMatches;
        }

        public string Hash(string password) => password;

        public bool Verify(string password, string passwordHash) => _passwordMatches;
    }

    private class FakeTokenService : ITokenService
    {
        public AccessToken Create(Guid userId, string email)
        {
            return new AccessToken("token", DateTime.UtcNow.AddHours(1));
        }
    }
}
