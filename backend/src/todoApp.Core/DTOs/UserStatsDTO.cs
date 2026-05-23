namespace todoApp.Core.DTOs;

public class UserStatsDTO
{
    public string Name { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public int TotalTasksCount { get; set; } 
    public DateTime? LastActivity { get; set; }
    public int CurrentStreak { get; set; } 
    public int LongestStreak { get; set; } 
    public double CompletionRate { get; set; }
    
}