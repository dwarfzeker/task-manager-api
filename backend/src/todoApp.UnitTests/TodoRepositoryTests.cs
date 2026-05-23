using Xunit;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using todoApp.Data.Repositories;
using todoApp.Data;
using todoApp.Core.Entities;

namespace todoApp.UnitTests;

public class TodoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TodoRepository _repository;
    public TodoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString() ).Options;
        _context = new AppDbContext(options);
        _repository = new TodoRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedData()
    {
        var user = new User
        {
            Id = "userwithjuicymelons",
            Name = "user1337",
            PasswordHash = "hashed",
            Email = "text@example.com"
        };
        _context.Users.Add(user);
        _context.Tasks.AddRange(
            new ListItem { Id = Guid.NewGuid(), UserId = "userwithjuicymelons", Title = "task1" },
            new ListItem { Id = Guid.NewGuid(), UserId = "userwithjuicymelons", Title = "task2" },
            new ListItem{Id = Guid.NewGuid(), UserId = "userwithoutjuicymelons", Title = "task3" }
        );
        _context.SaveChanges();
    }
    [Fact]
    public async Task GetAllByUserId_ShouldReturnUsersTask()
    {
        SeedData();
        var tasks = await _repository.GetByUserAsync("userwithjuicymelons");
        tasks.Should().HaveCount(2);
        tasks.All(t => t.UserId == "userwithjuicymelons").Should().BeTrue();
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask()
    {
        var id = Guid.NewGuid();
        await _repository.AddAsync(new ListItem { Id = id, UserId = "userwithjuicymelons", Title = "taskm" });
        var task = await _repository.GetByIdAsync(id);
        task.Should().NotBeNull();
        task.Title.Should().Be("taskm");
        task.UserId.Should().Be("userwithjuicymelons");
        
    }

    [Fact]
    public async Task AddAsync_ShouldAddTaskToDatabase()
    {
        var id = Guid.NewGuid();
        var task = new ListItem
        {
            Id = id,
            UserId = "userwithjuicymelons",
            Title = "task4"
        };
        await _repository.AddAsync(task);
        var savedTask = _repository.GetByIdAsync(task.Id);
        savedTask.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask()
    {
        var id = Guid.NewGuid();
        await _repository.AddAsync(new ListItem
        {
            Id = id,
            Title = "taskn",
            UserId = "userwithjuicymelons"
        });
        await _repository.DeleteAsync(id);
        var deletedTask = await _repository.GetByIdAsync(id);
        deletedTask.Should().BeNull();
    }
}