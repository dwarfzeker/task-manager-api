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
            new ListItem { Id = Guid.NewGuid(), UserId = "userwithjuicymelons", Title = "task1", TargetDate = DateTime.Today},
            new ListItem { Id = Guid.NewGuid(), UserId = "userwithjuicymelons", Title = "task2", TargetDate = DateTime.Today.AddDays(2)},
            new ListItem{Id = Guid.NewGuid(), UserId = "userwithoutjuicymelons", Title = "task3", TargetDate = DateTime.Today}
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
    public async Task GetAllAsync_ShouldReturnTasks()
    {
        SeedData();
        var tasks = await _repository.GetAllAsync();
        tasks.Should().HaveCount(3);
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
    public async Task GetByUserAndDateAsync_ShouldReturnTask()
    {
        SeedData();
        var tasks = await _repository.GetByUserAndDateAsync("userwithjuicymelons", DateTime.Today);
        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(1);
        tasks.First().Title.Should().Be("task1");
    }

    [Fact]
    public async Task UpdateAsync_WithoutDate_ShouldUpdate()
    {
        var taskId = Guid.NewGuid();
        var task = new ListItem
        {
            Id = taskId,
            Title = "task",
            TargetDate = null,
            CreatedAt = DateTime.Today
        };
        
        await _repository.AddAsync(task);
        var updatedTask = new ListItem
        {
            Id = taskId,
            Title = "newTitle",
            Description = "newDescription",
            TargetDate = null,
            CreatedAt = DateTime.Today
        };
        await _repository.UpdateAsync(updatedTask);
        var newTask = await _repository.GetByIdAsync(taskId);
        newTask.Should().NotBeNull();
        newTask.Title.Should().Be(updatedTask.Title);
        newTask.TargetDate.Should().Be(updatedTask.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WithDate_ShouldUpdate()
    {
        var taskId = Guid.NewGuid();
        var task = new ListItem
        {
            Id = taskId,
            Title = "task",
            TargetDate = null,
            CreatedAt = DateTime.Today
        };
        
        await _repository.AddAsync(task);
        var updatedTask = new ListItem
        {
            Id = taskId,
            Title = "newTitle",
            Description = "newDescription",
            TargetDate = DateTime.Today.AddDays(1),
            CreatedAt = DateTime.Today
        };
        await _repository.UpdateAsync(updatedTask);
        var newTask = await _repository.GetByIdAsync(taskId);
        newTask.Should().NotBeNull();
        newTask.Title.Should().Be(updatedTask.Title);
        newTask.TargetDate.Should().Be(updatedTask.TargetDate);
   
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTask_ShouldThrowInvalidOperation()
    {
        var taskId = Guid.NewGuid();
        var updatedTask = new ListItem
        {
            Id = taskId,
            Title = "newTitle",
            Description = "newDescription",
            TargetDate = DateTime.Today.AddDays(1),
            CreatedAt = DateTime.Today
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(updatedTask));
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

    [Fact]
    public async Task GetByDateAsync_ShouldReturnTasks()
    {
        SeedData();

        var tasks = await _repository.GetByDateAsync(DateTime.Today);
        
        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(2);
        tasks.First().Title.Should().Be("task1");
    }
    [Fact]
    public async Task Debug_Coverage_TestOnlyNullBranch()
    {
        // Этот тест специально для строки 54-57
        var taskId = Guid.NewGuid();
        var createdAt = DateTime.Today;
    
        var task = new ListItem
        {
            Id = taskId,
            Title = "Task",
            TargetDate = null,
            CreatedAt = createdAt
        };
    
        await _repository.AddAsync(task);
    
        _context.Entry(task).State = EntityState.Detached;
    
        var updated = new ListItem
        {
            Id = taskId,
            Title = "Updated",
            TargetDate = null,
            CreatedAt = createdAt
        };
    
        await _repository.UpdateAsync(updated);
    
        var result = await _repository.GetByIdAsync(taskId);
        Assert.Equal(createdAt.Date, result.TargetDate); 
    }
}