using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class CreateTaskDTO
{
    public string Title  { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public Guid? Id { get; set; }
    // public int? PositionIndex { get; set; }
    // public double? PositionX { get; set; }
    // public double? PositionY { get; set; }
}