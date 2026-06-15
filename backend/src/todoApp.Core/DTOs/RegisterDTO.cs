using  System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class RegisterDTO
{
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress] public string Email { get; set; } = string.Empty;
    

}

public class LoginDTO
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}