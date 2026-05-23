namespace todoApp.Core.DTOs;

public class CreateTaskDTO
{
    public string Title  { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    // public int? PositionIndex { get; set; }
    // public double? PositionX { get; set; }
    // public double? PositionY { get; set; }
}