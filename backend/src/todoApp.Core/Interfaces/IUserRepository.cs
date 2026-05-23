using todoApp.Core.Entities;
namespace todoApp.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByNameAsync(string userName);
    Task<User> CreateAsync(User user);
    Task<bool> UsernameExistsAsync(string username);
    Task<User?> UpdateAsync(User user);
}