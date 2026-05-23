namespace todoApp.Core.DTOs;

public class PatchTasksDTO
{
    public string? Title { get; set; } 
    public string? Description { get; set; } 
    public bool? IsCompleted { get; set; }
    
}