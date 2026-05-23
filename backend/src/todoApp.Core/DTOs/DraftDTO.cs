namespace todoApp.Core.DTOs;

public class DraftDTO
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserId { get; set; }
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime LasModifiedAt {get; set;} = DateTime.UtcNow;
    public DraftContext? Context { get; set; }
    public List<TaskDraftAction>?  Actions { get; set; }
    
}

public class TaskDraftAction
{
    public string ActionId { get; set; } = Guid.NewGuid().ToString();
    public DraftActionType ActionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TaskId { get; set; }
    public CreateTaskDTO? NewTaskData { get; set; }
    public UpdateTaskDTO ? UpdatedTaskData { get; set; }
}

public enum DraftActionType
{
    Create,
    Update,
    Delete,
    Complete,
    PositionChange
}

public class DraftContext
{
    public DateTime? SelectedDate {get; set;}
    public string? ActiveView { get; set; }
}