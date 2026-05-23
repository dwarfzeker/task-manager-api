using todoApp.Core.DTOs;
namespace todoApp.Core.Interfaces;

public interface ICachedStatsService
{
    Task<UserStatsDTO> GetUserStats(string userId);
    void InvalidateStats(string userId);
    
}