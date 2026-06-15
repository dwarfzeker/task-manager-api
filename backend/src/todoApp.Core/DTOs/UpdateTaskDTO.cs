using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class UpdateTaskDTO
{
    public string  Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public  bool IsCompleted { get; set; }
}