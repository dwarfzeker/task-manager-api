namespace todoApp.Core.DTOs;

public class UpdateTaskDTO
{
    public string  Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public  bool IsCompleted { get; set; }
}