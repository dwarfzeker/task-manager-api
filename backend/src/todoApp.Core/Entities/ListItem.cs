using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace todoApp.Core.Entities;

public class ListItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserId { get; set; }
    public DateTime? TargetDate { get; set; }
    public int? PositionIndex { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY {get;set;}
}