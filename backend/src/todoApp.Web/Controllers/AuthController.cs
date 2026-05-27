using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;
using todoApp.Core.DTOs;
namespace todoApp.Web.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    public AuthController(IUserRepository repository, IPasswordService passwordService, IJwtService jwtService)
    {
        _repository = repository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (await _repository.UsernameExistsAsync(dto.UserName)) return BadRequest("username already exists");
        var user = new User
        {
            Name = dto.UserName,
            PasswordHash = _passwordService.HashPassword(dto.Password),
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(user);
        var token = _jwtService.GenerateToken(user);
        Response.Cookies.Append("AccessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        });
        return Ok(new AuthResponseDTO
        {
            Token = token,
            UserName = user.Name,
            UserId = user.Id
        });
    }
    
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var user = await _repository.GetUserByNameAsync(dto.Login);
        if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }
        var token = _jwtService.GenerateToken(user);
        Response.Cookies.Append("AccessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        });
        return Ok(new AuthResponseDTO
        {
            Token = "stored in http-only cookie",
            UserName = user.Name,
            UserId = user.Id
        });
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AccessToken");
        return Ok("Logged out successfully");
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserName = User.Identity?.Name
        });
    }
}