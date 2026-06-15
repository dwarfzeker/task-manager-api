using todoApp.Core.DTOs;
namespace todoApp.Core.Interfaces;

public interface IStatsService
{
    Task<UserStatsDTO> GetUserStats(string userId);
    void InvalidateStats(string userId);
    
}