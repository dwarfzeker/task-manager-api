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
using Microsoft.AspNetCore.Http.Features;

using Xunit.Abstractions;
namespace todoApp.UnitTests;


public class TaskControllerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IEventDispatcher> _eventDispatcherMock;
    private readonly TaskController _controller;
    private readonly Mock<IEventHandler<TaskCompletedEvent>> _completedEventMock;
    private readonly Mock<IEventHandler<TaskDeletedEvent>> _deletedEventMock;
    private readonly Mock<IStatsService> _cachedStatsServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<TaskController>> _loggerMock;
    private readonly ITestOutputHelper _output;
    
    public TaskControllerTests(ITestOutputHelper output)
        {
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _eventDispatcherMock = new  Mock<IEventDispatcher>();
        _completedEventMock = new Mock<IEventHandler<TaskCompletedEvent>>();
        _deletedEventMock = new Mock<IEventHandler<TaskDeletedEvent>>();
        _cachedStatsServiceMock = new Mock<IStatsService>();
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
    public async Task GetAll_WithDate_ShouldReturnTasks()
    {
        var userId = "user1337";
        var taskId = Guid.NewGuid();
        var task = 
            new ListItem
            {
                Id = taskId,
                Title = "Test Task",
                UserId = userId,
                Description = "Test description",          
                TargetDate = DateTime.Today,                
                PositionX = 10.5,                           
                PositionY = 20.5,                                 
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            
            };
        var tasks = new List<ListItem> { task };
        var date = DateTime.Today;
        await _todoRepositoryMock.Object.AddAsync(task);
        _todoRepositoryMock.Setup(a => a.GetByUserAndDateAsync(userId, date)).ReturnsAsync(tasks);

        var result = await _controller.GetAll(date);
        _todoRepositoryMock.Verify(a => a.GetByUserAndDateAsync(userId, date), Times.Once);
        var okResult = result.Should().BeAssignableTo<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        var returnedTasks = okResult.Value.Should().BeAssignableTo<TakeListResponseDTO>().Subject;
        returnedTasks.Items.First().Title.Should().Be("Test Task");



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
    public async Task Get_WithWalidData_ShouldReturnOk()
    {
        var userId = "user1337";
        var taskId = Guid.NewGuid();
        var task = 
            new ListItem
            {
                Id = taskId,
                Title = "Test Task",
                UserId = userId,
                Description = "Test description",          
                TargetDate = DateTime.Today,                
                PositionX = 10.5,                           
                PositionY = 20.5,                                 
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            
            };
         await _todoRepositoryMock.Object.AddAsync(task);
         _todoRepositoryMock.Setup(a => a.GetByIdAsync(taskId)).ReturnsAsync(task);

         var result = await _controller.Get(taskId);
         _todoRepositoryMock.Verify(a => a.GetByIdAsync(taskId), Times.Once);
         var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
         var returnedTask = okResult.Value.Should().BeAssignableTo<TakeResponseDTO>().Subject;
         returnedTask.Title.Should().Be("Test Task");
    }
    [Fact]
    public async Task Get_WithInvalidUser_ShouldReturnForbid()
    {
        var userid = Guid.NewGuid().ToString();
        var taskid = Guid.NewGuid();
        var task = 
            new ListItem
            {
                Id = taskid,
                Title = "Test Task",
                UserId = "nonexistinuser",
                Description = "Test description",          
                TargetDate = DateTime.Today,                
                PositionX = 10.5,                           
                PositionY = 20.5,                                 
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            
            };
        await _todoRepositoryMock.Object.AddAsync(task); 
        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(taskid))
            .ReturnsAsync(task);
        var result = await _controller.Get(task.Id);
        _todoRepositoryMock.Verify(t => t.GetByIdAsync(taskid), Times.Once);
        result.Should().BeOfType<ForbidResult>();
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
    [Fact]
    public async Task Change_ChangeExistingTask_ShouldReturnOk()
    {
        var taskId = Guid.NewGuid();
        var existingTask = new ListItem 
        { 
            Id = taskId, 
            UserId = "user1337", 
            Title = "OldTitle",
            Description = "OldDescription",
            IsCompleted = false
        };
        var createDto = new CreateTaskDTO
        {
            Title = existingTask.Title,
            Id = taskId
        };
        var dto = new UpdateTaskDTO
        {
            Description = "newDescription",
            Title = "newTitle",
            IsCompleted = true
        };
        
         var createdResult = await _controller.Create(createDto);
         _todoRepositoryMock
             .Setup(r => r.GetByIdAsync(taskId))
             .ReturnsAsync(existingTask);
        _todoRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ListItem>()));
        var result = await _controller.Change(taskId, dto);
      
        
        createdResult.Should().BeOfType<CreatedAtActionResult>();
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeAssignableTo<ListItem>().Subject;
        result.Should().BeOfType<OkObjectResult>();
        okResult.Value.Should().NotBeNull();
        returnedTask.Description.Should().Be("newDescription");
        returnedTask.Title.Should().Be("newTitle");
        returnedTask.IsCompleted.Should().BeTrue();
        _todoRepositoryMock.Verify(r => r.UpdateAsync(existingTask), Times.Once);
        _todoRepositoryMock.Verify(r => r.UpdateAsync(It.Is<ListItem>(t => 
            t.Id == taskId && 
            t.Title == "newTitle" && 
            t.Description == "newDescription" &&
            t.IsCompleted == true
        )), Times.Once);
    } 
    [Fact]
    public async Task Change_NullDto_ShouldReturnBadRequest()
    {
        var taskId = Guid.NewGuid();
        var existingTask = new ListItem 
        { 
            Id = taskId, 
            UserId = "user1337", 
            Title = "OldTitle",
            Description = "OldDescription",
            IsCompleted = false
        };
        var createDto = new CreateTaskDTO
        {
            Title = existingTask.Title,
            Id = taskId
        };
        UpdateTaskDTO? dto = null;
        
        var createdResult = await _controller.Create(createDto);
        var result = await _controller.Change(taskId, dto);
      
        createdResult.Should().BeOfType<CreatedAtActionResult>();  
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Change_NullOldTask_ShouldReturnNotFound()
    {
        var taskId = Guid.NewGuid();
        var dto = new UpdateTaskDTO
        {
            Description = "newDescription",
            Title = "newTitle",
            IsCompleted = true
        };
        var result = await _controller.Change(taskId, dto);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RecentTasks_WithoutRecent_ShouldReturnOk()
    {
        // Arrange
        var userId = "user1337";
        SetupControllerWithSession(userId);
    
        _todoRepositoryMock
            .Setup(repo => repo.GetByUserAsync(userId))
            .ReturnsAsync(new List<ListItem>());
    
        // Act
        var result = await _controller.RecentTasks();
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        var response = okResult.Value;
        var tasksProperty = response!.GetType().GetProperty("tasks");
        var lastVisitProperty = response.GetType().GetProperty("lastVisit");
        var sessionIdProperty = response.GetType().GetProperty("sessionId");
        
        var tasks = tasksProperty?.GetValue(response) as IEnumerable<ListItem>;
        var lastVisit = lastVisitProperty?.GetValue(response) as string;
        var sessionId = sessionIdProperty?.GetValue(response) as string;

        tasks.Should().NotBeNull();
        tasks.Count().Should().Be(0);
        lastVisit.Should().Be("First visit!");
        sessionId.Should().NotBeNull();
    }
    
    [Fact]
    public void SaveDraft_WithExistingUser_ShouldReturnOk()
    {
        var userId = "user1337";
        
        SetupControllerWithSession(userId);
        var dto = new DraftDTO
        {
            Context = new DraftContext(),
            CreatedAt = DateTime.UtcNow,
            Actions = new List<TaskDraftAction>()
        };
        
        
        var result = _controller.SaveDraft(dto);
        
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value;
        var savedProperty = response!.GetType().GetProperty("saved");
        var saved = savedProperty!.GetValue(response) as bool?;
        saved.Should().NotBeNull();
        saved.Should().BeTrue();
    }

    [Fact]
    public void GetDraft_WithValidUser_ShouldReturnOk()
    {
        var userId = "user1337";
        SetupControllerWithSession(userId);
        var dto = new DraftDTO
        {
            Context = new DraftContext(),
            CreatedAt = DateTime.UtcNow,
            Actions = new List<TaskDraftAction>()
        };
        
        
        var save = _controller.SaveDraft(dto);

        var result = _controller.GetDraft();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task ToggleCompleted_WithValidTask_ShouldReturnTask()
    {
        var userId = "user1337";
        var taskId = Guid.NewGuid();
        var task = new ListItem 
        { 
            Id = taskId, 
            UserId = "user1337", 
            Title = "OldTitle",
            Description = "OldDescription",
            IsCompleted = false
        };
        SetupControllerWithSession(userId);
        var dto = new PatchTasksDTO
        {
            Title = task.Title,
            Description = task.Description,
            IsCompleted = !task.IsCompleted
        };

        await _todoRepositoryMock.Object.AddAsync(task);
        _todoRepositoryMock.Setup(t => t.GetByIdAsync(taskId)).ReturnsAsync(task);
        _completedEventMock.Setup(t => t.HandleAsync(It.IsAny<TaskCompletedEvent>()));
        var result = await _controller.ToggleCompleted(taskId, dto);
        _todoRepositoryMock.Verify(t => t.GetByIdAsync(taskId), Times.Once);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
       // var response = okResult.Value;
        var returnedtask = okResult.Value.Should().BeAssignableTo<ListItem>().Subject;
        returnedtask.Title.Should().NotBeNull();
        returnedtask.Title.Should().Be("OldTitle");
        returnedtask.IsCompleted.Should().BeTrue();

    }
    [Fact]
    public async Task ToggleCompleted_WithCompletedFirst_ShouldReturnTask()
    {
        var userId = "user1337";
        var taskId = Guid.NewGuid();
        var task = new ListItem 
        { 
            Id = taskId, 
            UserId = "user1337", 
            Title = "OldTitle",
            Description = "OldDescription",
            IsCompleted = true
        };
        SetupControllerWithSession(userId);
        var dto = new PatchTasksDTO
        {
            Title = task.Title,
            Description = task.Description,
            IsCompleted = !task.IsCompleted
        };

        await _todoRepositoryMock.Object.AddAsync(task);
        _todoRepositoryMock.Setup(t => t.GetByIdAsync(taskId)).ReturnsAsync(task);
        _todoRepositoryMock.Setup(t => t.UpdateAsync(task));
        var result = await _controller.ToggleCompleted(taskId, dto);
        _todoRepositoryMock.Verify(t => t.GetByIdAsync(taskId), Times.AtMost(2));
        _todoRepositoryMock.Verify(t => t.UpdateAsync(task), Times.AtMost(2));
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value;
        var taskresult = okResult.Value.Should().BeAssignableTo<ListItem>().Subject;
        taskresult.Title.Should().NotBeNull();
        taskresult.Title.Should().Be("OldTitle");
        taskresult.IsCompleted.Should().BeFalse();

    }

    [Fact]
    public async Task ToggleCompleted_WithInvalidtask_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _todoRepositoryMock.Setup(a => a.GetByIdAsync(id));
        var result = await _controller.ToggleCompleted(id, null);
        _todoRepositoryMock.Verify(a => a.GetByIdAsync(id), Times.Once);
        result.Should().BeOfType<NotFoundResult>();

    }
    [Fact]
    public async Task UpdatePosition_WithWalidTask_ShouldReturnOk()
    {
        var userId = "user1337";
        var taskId = Guid.NewGuid();
        var task = new ListItem 
        { 
            Id = taskId, 
            UserId = userId, 
            Title = "OldTitle",
            Description = "OldDescription",
            IsCompleted = false,
        };
        

        var dto = new UpdatePositionDTO
        {
            Id = taskId,
            PositionX = 10,
            PositionY = 10
        };
        await _todoRepositoryMock.Object.AddAsync(task);
        _todoRepositoryMock.Setup(t => t.GetByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _controller.UpdatePosition(taskId, dto);
        _todoRepositoryMock.Verify(t => t.GetByIdAsync(taskId), Times.Once);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        var response = okResult.Value;
        var idPorperty = response!.GetType().GetProperty("Id");
        var id = idPorperty!.GetValue(response) as Guid?;
        id.Should().NotBeNull();
        id.Should().Be(task.Id);

    }
    private void SetupControllerWithSession(string userId, Dictionary<string, byte[]>? sessionData = null)
    {
        var sessionMock = new Mock<ISession>();
        var storage = sessionData ?? new Dictionary<string, byte[]>();
    
        sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
            .Returns((string key, out byte[]? value) => storage.TryGetValue(key, out value));
    
        sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => storage[key] = value);
    
        sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
            .Callback<string>(key => storage.Remove(key));
    
        sessionMock.Setup(s => s.Clear())
            .Callback(() => storage.Clear());
    
        sessionMock.Setup(s => s.Id).Returns(Guid.NewGuid().ToString());
        sessionMock.Setup(s => s.IsAvailable).Returns(true);
    
        var httpContext = new DefaultHttpContext();
    
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(sessionMock.Object));
    
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }



    private class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; }
    
        public TestSessionFeature(ISession session)
        {
            Session = session;
        }
    }
}