using todoApp.Core.Entities;
using todoApp.Core.Interfaces;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace todoApp.Core.Services;

public class JwtService: IJwtService
{
    private readonly string _secret;
    private readonly string _isuuer;
    private readonly string _audience;

    public JwtService(IConfiguration configuration)
    {
        _secret = configuration["JwtSettings:Secret"] ?? configuration["Jwt:Secret"];
        _isuuer = configuration["Jwt:Issuer"] ?? "TodoApp";
        _audience = configuration["Jwt:Audience"] ?? "TodoAppUsers";
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Name)
        };
        var token = new JwtSecurityToken(
            issuer: _isuuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
            
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}