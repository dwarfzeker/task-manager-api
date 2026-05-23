using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace todoApp.Core.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string? Email { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public ICollection<ListItem> Tasks { get; set; } =  new List<ListItem>();
    public int CompletedTasks { get; set; } = 0;
    public int TotalTasksCount { get; set; } = 0;
    public DateTime? LasActivity { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;

}