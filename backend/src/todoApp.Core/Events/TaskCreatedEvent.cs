using todoApp.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace todoApp.Core.Events;

[ExcludeFromCodeCoverage]
public class TaskCreatedEvent : IDomainEvent
{
    public string UserId { get; set; }
    public string TaskId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;

    public TaskCreatedEvent(string userId, string taskId, DateTime createdAt)
    {
        UserId = userId;
        TaskId = taskId;
        CreatedAt = createdAt;
        
    }
}