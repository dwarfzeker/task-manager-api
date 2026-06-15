using todoApp.Core.Entities;
using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class TakeListResponseDTO
{
    public IEnumerable<TakeResponseDTO> Items { get; set; } = new List<TakeResponseDTO>();
    public int TotalCount { get; set; }
    public int? DayCount { get; set; }
    public DateTime? Date { get; set; }


    public static TakeListResponseDTO FromTasks(List<ListItem> tasks, DateTime? date = null)
    {
        return new TakeListResponseDTO
        {
            Items = tasks.Select(TakeResponseDTO.FromEntity).ToList(),
            TotalCount = tasks.Count,
            DayCount = date.HasValue ? tasks.Count : null,
            Date = date,
            
        }
        ;
    }
}