using Microsoft.AspNetCore.Authorization;
using Nstech.OrderService.Api;
using Microsoft.AspNetCore.Mvc;
using Nstech.OrderService.Application.Auth;

namespace Nstech.OrderService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await _auth.Login(request, cancellationToken);
        return Ok(ApiResponse<TokenResponse>.Ok(token));
    }
}
