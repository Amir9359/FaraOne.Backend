using FaraOne.Infraustructor;
using Microsoft.AspNetCore.Mvc;

namespace FaraOne.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.Authenticate(request.Username, request.Password);

        if (user == null)
            return Unauthorized(new { message = "Username or password is incorrect" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Name,
                user.Role,
                user.Avatar
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.Register(request.Username, request.Email, request.Password, request.FullName);
            var token = _authService.GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Name,
                    user.Role
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}