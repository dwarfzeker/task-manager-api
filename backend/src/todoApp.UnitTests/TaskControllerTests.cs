using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using todoApp.Core.Entities;
using todoApp.Core.Interfaces;
using todoApp.Core.DTOs;
using todoApp.Web.Controllers;
using todoApp.Core.Events;
using System.Threading.Tasks;
using System;
using Xunit;
using System.Collections.Generic;
using Xunit.Abstractions;
namespace todoApp.UnitTests;


public class TaskControllerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IEventDispatcher> _eventDispatcherMock;
    private readonly TaskController _controller;
    private readonly Mock<IEventHandler<TaskCompletedEvent>> _completedEventMock;
    private readonly Mock<IEventHandler<TaskDeletedEvent>> _deletedEventMock;
    private readonly Mock<ICachedStatsService> _cachedStatsServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<TaskController>> _loggerMock;
    private readonly ITestOutputHelper _output;
    
    public TaskControllerTests(ITestOutputHelper output)
        {
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _eventDispatcherMock = new  Mock<IEventDispatcher>();
        _completedEventMock = new Mock<IEventHandler<TaskCompletedEvent>>();
        _deletedEventMock = new Mock<IEventHandler<TaskDeletedEvent>>();
        _cachedStatsServiceMock = new Mock<ICachedStatsService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<TaskController>>(); 
        _controller = new TaskController(
            _todoRepositoryMock.Object,
            _loggerMock.Object,
            _completedEventMock.Object,
            _deletedEventMock.Object,
            _cachedStatsServiceMock.Object,
            _userRepositoryMock.Object);
            
            SetupControllerContext(_controller, "user1337");
            _output = output;
        }

    private void SetupControllerContext(TaskController controller, string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnTasks_WhenTasksExists()
    {
        var task = 
            new ListItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                UserId = "user1337",
                Description = "Test description",          
                TargetDate = DateTime.Today,                
                PositionX = 10.5,                           
                PositionY = 20.5,                                 
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            
        };
        var tasks = new List<ListItem> { task };
        await _todoRepositoryMock.Object.AddAsync(task);
        _todoRepositoryMock.Setup(r => r.GetByUserAsync("user1337")).ReturnsAsync(tasks);

        var result = await _controller.GetAll(null);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        var returnedTasks = okResult.Value.Should().BeAssignableTo<TakeListResponseDTO>().Subject;
        returnedTasks.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _todoRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ListItem?)null);
        var result = await _controller.Get(id);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreated()
    {
        var dto = new CreateTaskDTO
        {
            Title = "task's title",
            Description = "task's description",
            TargetDate = DateTime.UtcNow
        };
        ListItem? capturedTask = null;
        _todoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ListItem>()))
            .Callback<ListItem>(task =>
            {
                _output.WriteLine($"AddAsync called with: '{task.Title}'");
                capturedTask = task;
            })
            .Returns(Task.CompletedTask);
        var result = await _controller.Create(dto);
        
        _output.WriteLine($"Result type: {result.GetType().Name}");
        result.Should().BeOfType<CreatedAtActionResult>();
        _todoRepositoryMock.Verify(r => r.AddAsync(It.Is<ListItem>(t => t.Title == "task's title")), Times.Once);
        capturedTask.Should().NotBeNull();
        capturedTask.Title.Should().Be("task's title");
        capturedTask.Description.Should().Be("task's description");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task Create_WithInvalidTitle_ShouldReturnBadRequest(string? title)
    {
        var dto = new CreateTaskDTO { Title = title! };
        var result = await _controller.Create(dto);
        result.Should().BeOfType<BadRequestObjectResult>();
        
    }
    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        var taskId = Guid.NewGuid();
        var todo = new ListItem
        {
            Id = taskId,
            Title = "task's title",
            Description = "task's description",
            TargetDate = DateTime.UtcNow,
            UserId = "user1337"
        };
        _todoRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(todo);
        var result = await _controller.Delete(taskId);
        result.Should().BeOfType<NoContentResult>();
        _todoRepositoryMock.Verify(r => r.DeleteAsync(taskId), Times.Once);
    }
    [Fact]
    public async Task Delete_TaskOfAnotherUser_ShouldReturnNotFound()
    {
        var taskId = Guid.NewGuid();
        var task = new ListItem { Id = taskId, UserId = "anotheruser" };
        _todoRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        var result = await _controller.Delete(taskId);
        result.Should().BeOfType<NotFoundResult>();
    }
    
    
    
    
}