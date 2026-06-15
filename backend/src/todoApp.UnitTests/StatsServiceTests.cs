using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using todoApp.Core.Services;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;

namespace todoApp.UnitTests;

public class StatsServiceTests
{
    private readonly StatsService _service;
    private readonly Mock<ITodoRepository> _repositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ILogger<StatsService>> _loggerMock;
    public StatsServiceTests()
    {
        _repositoryMock = new Mock<ITodoRepository>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<StatsService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new StatsService(_memoryCacheMock.Object, _repositoryMock.Object,  _loggerMock.Object, _userRepositoryMock.Object);
        
    }

    [Fact]
    public async Task GetUserStats_WithValidUser_ReturnsStats()
    {
        var user = new User
        {
            Name = "vasily",
            TotalTasksCount = 4,
            CompletedTasks = 2,
            CreatedAt = DateTime.Today.AddDays(-7),
            CurrentStreak = 2,
            Email = "example@example.com",
            Id = Guid.NewGuid().ToString(),
            LongestStreak = 2,
            PasswordHash = "hash-123-password",
            LasActivity = DateTime.Today
        };
        var tasks = new List<ListItem>
        {
            new ListItem { Id = Guid.NewGuid(), Title = "task1", IsCompleted = true, UserId = user.Id },
            new ListItem { Id = Guid.NewGuid(), Title = "task2", IsCompleted = true, UserId = user.Id },
            new ListItem { Id = Guid.NewGuid(), Title = "task3", IsCompleted = false, UserId = user.Id },
            new ListItem { Id = Guid.NewGuid(), Title = "task4", IsCompleted = false, UserId = user.Id },

        };
        await _userRepositoryMock.Object.CreateAsync(user);
        await _repositoryMock.Object.AddAsync(tasks[0]);
        await _repositoryMock.Object.AddAsync(tasks[1]);
        await _repositoryMock.Object.AddAsync(tasks[2]);
        await _repositoryMock.Object.AddAsync(tasks[3]);
        _repositoryMock.Setup(a => a.GetByUserAsync(user.Id)).ReturnsAsync(tasks);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.GetUserStats(user.Id);
        _repositoryMock.Verify(r => r.GetByUserAsync(user.Id), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(user.Id), Times.Once);
        result.Name.Should().Be("vasily");
        result.CompletedTasks.Should().Be(2);
        result.CompletionRate.Should().Be(50);
        result.CurrentStreak.Should().Be(2);
        result.LastActivity.Should().Be(DateTime.Today);


    }
    [Fact]
    public async Task GetUserStats_WithoutTasks_ReturnsStats()
    {
        var user = new User
        {
            Name = "vasily",
            TotalTasksCount = 4,
            CompletedTasks = 2,
            CreatedAt = DateTime.Today.AddDays(-7),
            CurrentStreak = 2,
            Email = "example@example.com",
            Id = Guid.NewGuid().ToString(),
            LongestStreak = 2,
            PasswordHash = "hash-123-password",
            LasActivity = DateTime.Today
        };
        var tasks = new List<ListItem>();
        await _userRepositoryMock.Object.CreateAsync(user);
        _repositoryMock.Setup(a => a.GetByUserAsync(user.Id)).ReturnsAsync(tasks);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.GetUserStats(user.Id);
        _repositoryMock.Verify(r => r.GetByUserAsync(user.Id), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(user.Id), Times.Once);
        result.Name.Should().Be("vasily");
        result.CompletedTasks.Should().Be(0);
        result.CompletionRate.Should().Be(0);
        result.CurrentStreak.Should().Be(2);
        result.LastActivity.Should().Be(DateTime.Today);


    }

    [Fact]
    public async Task GetUserStats_NonexistentUser_ThrowsException()
    {
        var userId = Guid.NewGuid().ToString();
   
        await Assert.ThrowsAsync<Exception>(async () => await _service.GetUserStats(userId));
        
    }

    [Fact]
    public void InvalidateStats_ShouldRemoveCache()
    {
        var userId = Guid.NewGuid().ToString();
        var cacheKey = $"stats_{userId}";
        _memoryCacheMock.Setup(c => c.Remove(cacheKey));
        
        _service.InvalidateStats(userId);
        _memoryCacheMock.Verify(c => c.Remove(cacheKey), Times.Once);
    }
}