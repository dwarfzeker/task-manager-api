using todoApp.Core.Interfaces;

namespace todoApp.Core.Events;

public class TaskDeletedEvent : IDomainEvent
{
    public string TaskId { get; set; }
    public string UserId { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime OccuredAt { get; set; } = DateTime.UtcNow;

    public TaskDeletedEvent(string TaskId, string UserId, DateTime DeletedAt)
    {
        this.TaskId = TaskId;
        this.UserId = UserId;
        this.DeletedAt = DeletedAt;
    }
}