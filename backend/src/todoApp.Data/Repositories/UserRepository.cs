using Microsoft.EntityFrameworkCore;
using todoApp.Core.Entities;
using todoApp.Core.Interfaces;

namespace todoApp.Data.Repositories;

public class UserRepository :  IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByNameAsync(string userName)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == userName);
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await GetUserByNameAsync(username) != null;
    }

    public async Task<User?> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}