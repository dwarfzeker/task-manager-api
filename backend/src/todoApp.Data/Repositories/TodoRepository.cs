using Microsoft.EntityFrameworkCore;
using todoApp.Core.Entities;
using todoApp.Core.Interfaces;
using System.Linq;
using System.Collections;
namespace todoApp.Data.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;
    public  TodoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ListItem>> GetAllAsync()
    {
        return await _context.Tasks.ToListAsync();
    }

    public async Task<ListItem?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<List<ListItem>> GetByUserAsync(string userId)
    {
        return await _context.Tasks.Where(t => t.UserId == userId).ToListAsync<ListItem>();
    }

    public async Task<List<ListItem>> GetByUserAndDateAsync(string userId, DateTime date)
    {
        return await _context.Tasks.Where(t => t.UserId == userId && t.TargetDate == date).ToListAsync<ListItem>();
        
    }
    
    public async Task AddAsync(ListItem item)
    {
        await _context.Tasks.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ListItem item)
    {
        var oldTask = await GetByIdAsync(item.Id);
        if (oldTask == null) throw new InvalidOperationException("task not found");
        oldTask.Title = item.Title;
        oldTask.Description = item.Description;
        oldTask.TargetDate = item.TargetDate;
        oldTask.IsCompleted = item.IsCompleted;
        oldTask.PositionX = item.PositionX;
        oldTask.PositionY = item.PositionY;
        oldTask.CreatedAt = item.CreatedAt;
        if (oldTask?.TargetDate == null)
        {
            oldTask!.TargetDate = item.CreatedAt.Date;
        }
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Guid id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.Tasks.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ListItem>> GetByDateAsync(DateTime date)
    {
        return await _context.Tasks.Where(t => t.TargetDate == date.Date && t.TargetDate < date.Date.AddDays(1) ).ToListAsync();
    }
}