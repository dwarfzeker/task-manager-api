using Castle.Core.Logging;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using todoApp.Core.Interfaces;
using todoApp.Data.Handlers;
using todoApp.Data;
using todoApp.Core.Entities;
using todoApp.Core.Events;
using Microsoft.EntityFrameworkCore;
using EventHandler = todoApp.Data.Handlers.EventHandler;

namespace todoApp.UnitTests;

public class EventHandlerTests : IDisposable
{
      private readonly AppDbContext _dbContext;
      private readonly Mock<ILogger<Data.Handlers.EventHandler>> _mockLogger;
      private readonly Mock<IUserRepository> _repositoryMock;
      private readonly EventHandler _handler;
      private readonly Mock<ITodoRepository> _todoRepositoryMock;

      public EventHandlerTests()
      {       
          var options = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
              .Options;
          _dbContext = new AppDbContext(options);
          _repositoryMock = new Mock<IUserRepository>();
          _mockLogger = new Mock<ILogger<EventHandler>>();
          _todoRepositoryMock = new Mock<ITodoRepository>();
          _handler = new EventHandler(_dbContext, _mockLogger.Object, _repositoryMock.Object);
      }

      [Fact]
      public async Task HandleAsync_CompletedTask_WithUser_ShouldHandle()
      {
          var user = new User
          {
              Id = "user1337",
              Name = "Vasily",
              Email = "example@example.com",
              CompletedTasks = 4,
              CurrentStreak = 2,
              LongestStreak = 2,
              LasActivity = DateTime.Today.AddDays(-1)
          };
          var yeasterdayTask = new ListItem
          {
              Title = "yeasterdaytask",
              Id = Guid.NewGuid(),
              CreatedAt = DateTime.Today.AddDays(-1)
          };
          await _dbContext.Tasks.AddAsync(yeasterdayTask);
          var @event = new TaskCompletedEvent(taskId: Guid.NewGuid().ToString(), userId: user.Id,
              completedAt: DateTime.UtcNow);
          await _dbContext.Users.AddAsync(user);
          await _dbContext.SaveChangesAsync();
          _repositoryMock.Setup(u => u.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
         await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(user.Id), Times.Once);
          var updatedUser = await _dbContext.Users.FindAsync(user.Id);
          updatedUser!.CompletedTasks.Should().Be(user.CompletedTasks);
          user.LasActivity.Should().Be(@event.CompletedAt);
      }
      [Fact]
      public async Task HandleAsync_CompletedTask_WithoutUser_ShouldDoNothing()
      {
          var userId = Guid.NewGuid().ToString();
          var @event = new TaskCompletedEvent(taskId: Guid.NewGuid().ToString(), userId: userId,
              completedAt: DateTime.UtcNow);
          _repositoryMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync((User)null);
          await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
          var user = _repositoryMock.Object.GetUserByIdAsync(userId).Result;
          user.Should().BeNull();
      }
      [Fact]
      public async Task HandleAsync_CreatedTask_WithUser_ShouldHandle()
      {
          var user = new User
          {
              Id = "user1337",
              Name = "Vasily",
              Email = "example@example.com",
              CompletedTasks = 6,
              CurrentStreak = 2,
              LongestStreak = 4,
              LasActivity = DateTime.Today.AddDays(-1)
          };
          var @event = new TaskCreatedEvent(taskId: Guid.NewGuid().ToString(), userId: user.Id,
              createdAt: DateTime.UtcNow);
          await _dbContext.Users.AddAsync(user);
          await _dbContext.SaveChangesAsync();
          _repositoryMock.Setup(u => u.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
          await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(user.Id), Times.Once);
          var updatedUser = await _dbContext.Users.FindAsync(user.Id);
          updatedUser!.CompletedTasks.Should().Be(user.CompletedTasks);
          user.LasActivity.Should().Be(@event.CreatedAt);
      }
      [Fact]
      public async Task HandleAsync_CreatedTask_WithoutUser_ShouldDoNothing()
      {
          var userId = Guid.NewGuid().ToString();
          var @event = new TaskCreatedEvent(taskId: Guid.NewGuid().ToString(), userId: userId,
              createdAt: DateTime.UtcNow);
          _repositoryMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync((User)null);
          await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
          var user = _repositoryMock.Object.GetUserByIdAsync(userId).Result;
          user.Should().BeNull();
      }
      [Fact]
      public async Task HandleAsync_DeletedTask_WithUser_ShouldHandle()
      {
          var user = new User
          {
              Id = "user1337",
              Name = "Vasily",
              Email = "example@example.com",
              CompletedTasks = 4,
              CurrentStreak = 2,
              LasActivity = DateTime.Today.AddDays(-1)
          };
          var @event = new TaskDeletedEvent(TaskId: Guid.NewGuid().ToString(), UserId: user.Id, DeletedAt: DateTime.UtcNow);
          await _dbContext.Users.AddAsync(user);
          await _dbContext.SaveChangesAsync();
          _repositoryMock.Setup(u => u.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
          await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(user.Id), Times.Once);
          var updatedUser = await _dbContext.Users.FindAsync(user.Id);
          updatedUser!.CompletedTasks.Should().Be(user.CompletedTasks);
          user.LasActivity.Should().Be(@event.DeletedAt);
      }
      [Fact]
      public async Task HandleAsync_DeletedTask_WithoutUser_ShouldDoNothing()
      {
          var userId = Guid.NewGuid().ToString();
          var @event = new TaskDeletedEvent(TaskId: Guid.NewGuid().ToString(), UserId: userId,
              DeletedAt: DateTime.UtcNow);
          _repositoryMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync((User)null);
          await _handler.HandleAsync(@event);
          _repositoryMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
          var user = _repositoryMock.Object.GetUserByIdAsync(userId).Result;
          user.Should().BeNull();
      }
      public void Dispose()
      {
          _dbContext.Dispose();
      }
}