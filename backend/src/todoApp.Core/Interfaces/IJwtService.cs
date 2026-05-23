using todoApp.Core.Entities;
namespace todoApp.Core.Interfaces;

public interface IJwtService
{
    public string GenerateToken(User user);
    
}