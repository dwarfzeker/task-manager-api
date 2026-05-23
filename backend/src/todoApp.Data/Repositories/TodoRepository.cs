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
        if (item.TargetDate == null)
        {
            item.TargetDate = item.CreatedAt.Date;
        }
        _context.Tasks.Update(item);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateRangeAsync(List<ListItem> items)
    {
        foreach (var item in items)
        {
            if (item.TargetDate == null)
            {
                item.TargetDate = item.CreatedAt.Date;
            }
        }
        _context.Tasks.UpdateRange(items);
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


    public async Task<List<int?>> GetPositionIndicesByDateAsync(DateTime date)
    {
        var thisDay = await GetByDateAsync(date);
        var indices = thisDay.Select(t => t.PositionIndex).ToList();
        return indices;
    }
}