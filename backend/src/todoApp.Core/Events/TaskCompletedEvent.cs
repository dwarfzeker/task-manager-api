using todoApp.Core.Interfaces;

namespace todoApp.Core.Events;

public class TaskCompletedEvent : IDomainEvent
{
    public string UserId { get; set; }
    public string TaskId { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;

    public TaskCompletedEvent(string taskId, string userId, DateTime completedAt)
    {
        TaskId = taskId;
        UserId = userId;
        CompletedAt = completedAt;
    }
}