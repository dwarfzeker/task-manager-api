using todoApp.Core.Entities;

namespace todoApp.Core.Interfaces;

public interface ITodoRepository
{
    Task<List<ListItem>> GetAllAsync();
    Task<ListItem?> GetByIdAsync(Guid id);
    Task AddAsync(ListItem item);
    Task UpdateAsync(ListItem item);
    Task DeleteAsync(Guid id);
    Task<List<ListItem>> GetByDateAsync(DateTime date);
    
    Task<List<ListItem>> GetByUserAsync(string userId);

    Task<List<ListItem>> GetByUserAndDateAsync(string userId, DateTime date);
}