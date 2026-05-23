using todoApp.Core.Interfaces;
using todoApp.Core.Events;


namespace todoApp.Data.Handlers;

public class NotificationHandler : IEventHandler<TaskCompletedEvent>, IEventHandler<TaskCreatedEvent>
{
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;
    
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(ILogger<NotificationHandler> logger)
    {
        _logger = logger;
    }
    public  Task HandleAsync(TaskCompletedEvent taskCompletedEvent)
    {
        _logger.LogInformation($"TaskCompletedEvent: {taskCompletedEvent}");
        return Task.CompletedTask;
    }

    public Task HandleAsync(TaskCreatedEvent createdEvent)
    {
        _logger.LogInformation($"TaskCreatedEvent: {createdEvent}");
        return Task.CompletedTask;
    }
}