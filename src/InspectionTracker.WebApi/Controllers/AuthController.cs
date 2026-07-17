using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Application.Services;
using InspectionTracker.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InspectionTracker.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService, IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = await authService.RegisterAsync(
            new RegisterDto(request.Email, request.DisplayName, request.Password), ct);

        return Created(string.Empty,
            new AuthResponse(tokenService.CreateToken(user), user.Email, user.DisplayName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await authService.LoginAsync(new LoginDto(request.Email, request.Password), ct);

        return Ok(new AuthResponse(tokenService.CreateToken(user), user.Email, user.DisplayName));
    }
}
