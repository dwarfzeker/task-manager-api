using Microsoft.EntityFrameworkCore;
using todoApp.Core.Events;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;
namespace todoApp.Data.Handlers;

public class EventHandler : IEventHandler<TaskCompletedEvent>, IEventHandler<TaskCreatedEvent>, IEventHandler<TaskDeletedEvent>
{
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EventHandler> _logger;
    private readonly IUserRepository _userRepository;
    public EventHandler(AppDbContext dbContext, ILogger<EventHandler> logger,  IUserRepository userRepository)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task HandleAsync(TaskCompletedEvent @event)
    {
        _logger.LogInformation($"Handling task {@event.TaskId} for user {@event.UserId}");
        var user = await _userRepository.GetUserByIdAsync(@event.UserId);
        if (user != null)
        {
            user.CompletedTasks++;
            
            user.LasActivity = @event.CompletedAt;
            await _dbContext.SaveChangesAsync();

            await UpdateStreakAsync(user, @event.CompletedAt);
            _logger.LogInformation($"Task {@event.TaskId} completed {@event.UserId}");
        } else _logger.LogError($"user {@event.UserId} could not be found");
    }

    public async Task HandleAsync(TaskCreatedEvent @event)
    {
        
        _logger.LogInformation($"creating task {@event.TaskId} for user {@event.UserId}");
        var user = await _userRepository.GetUserByIdAsync(@event.UserId);
        if (user != null)
        {
            user.TotalTasksCount++;
            user.LasActivity = @event.CreatedAt;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Task {@event.TaskId} created for {@event.UserId}");
        }
    }
    
    public async Task HandleAsync(TaskDeletedEvent @event)
    {
        
        _logger.LogInformation($"deleting task {@event.TaskId} for user {@event.UserId}");
        var user = await _userRepository.GetUserByIdAsync(@event.UserId);
        
        if (user != null)
        {
            user.TotalTasksCount = Math.Max(user.TotalTasksCount - 1, 0);
            user.LasActivity = @event.DeletedAt;
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation($"Task {@event.TaskId} created for {@event.UserId}");
        }
    }

    private async Task UpdateStreakAsync(User user, DateTime date)
    {
        var today = date.Date;
        var yesterday = today.AddDays(-1);
        var hadActivityYesterday =
            await _dbContext.Tasks.AnyAsync(t => t.UserId == user.Id && t.IsCompleted && t.CreatedAt.Date == yesterday);
        if (hadActivityYesterday)
        {
            user.CurrentStreak++;
        }
        else 
        {
            user.CurrentStreak = 1;
        }
        
        user.LongestStreak = Math.Max(user.LongestStreak, user.CurrentStreak);
        await _userRepository.UpdateAsync(user);
    }
    
}