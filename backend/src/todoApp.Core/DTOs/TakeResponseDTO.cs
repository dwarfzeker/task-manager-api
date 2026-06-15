using System.Runtime.InteropServices.JavaScript;
using todoApp.Core.Entities;
using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class TakeResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } 
    public DateTime CreatedAt { get; set; }
    public int? PositionIndex { get; set; }
    public DateTime? TargetDate { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }


    public static TakeResponseDTO FromEntity(ListItem entity)
    {
        return new TakeResponseDTO
        {
            Id = entity.Id,
            Title =  entity.Title,
            Description = entity.Description,
            IsCompleted = entity.IsCompleted,
            CreatedAt = entity.CreatedAt,
            PositionIndex = entity.PositionIndex,
            TargetDate = entity.TargetDate ?? DateTime.Today,
            PositionX = entity.PositionX,
            PositionY = entity.PositionY
        };
    }
}