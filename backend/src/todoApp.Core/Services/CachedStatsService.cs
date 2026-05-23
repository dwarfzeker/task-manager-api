using Microsoft.Extensions.Caching.Memory;
using todoApp.Core.DTOs;
using todoApp.Core.Entities;
using todoApp.Core.Interfaces;

namespace todoApp.Core.Services;

public class CachedStatsService : ICachedStatsService
{
    private readonly IMemoryCache _cache;
    private readonly ITodoRepository _todoRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CachedStatsService> _logger;

    public CachedStatsService(IMemoryCache cache, ITodoRepository todoRepository, ILogger<CachedStatsService> logger, IUserRepository userRepository)
    {
        _cache = cache;
        _todoRepository = todoRepository;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<UserStatsDTO> GetUserStats(string userId)
    {
        var cacheKey = "stats_{userId}";
        if (_cache.TryGetValue(cacheKey, out UserStatsDTO? stats))
        {
            _logger.LogInformation($"stats for user {userId} got from cache successfully");
            return stats!;
        }
        _logger.LogInformation($"computing stats for user {userId}");
        var tasks =  await _todoRepository.GetByUserAsync(userId);
        var user = _userRepository.GetUserByIdAsync(userId).Result ?? throw new Exception($"User {userId} not found");
        var newstats = new UserStatsDTO
        {
            Name = user.Name,
            TotalTasksCount = tasks.Count(),
            CompletedTasks = tasks.Count(t => t.IsCompleted),
            CompletionRate = tasks.Any() ? Math.Round((double) tasks.Count(t => t.IsCompleted) / tasks.Count()) * 100 : 0,
            LongestStreak = user.LongestStreak,
            CurrentStreak =  user.CurrentStreak,
            LastActivity =  user.LasActivity ?? DateTime.UtcNow
        };
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogInformation($"Cache: {key}, Evicted: {reason}");
            });

        _cache.Set(cacheKey, newstats, cacheOptions);
        return newstats;
    }

    public void InvalidateStats(string userId)
    {
        var cacheKey = $"stats_{userId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation($"InvalidateStats for user {userId}");
    }
}