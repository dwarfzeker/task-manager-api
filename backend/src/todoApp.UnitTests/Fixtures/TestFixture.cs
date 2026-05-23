using Microsoft.EntityFrameworkCore;
using todoApp.Data;
using todoApp.Data.Repositories;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
namespace todoApp.UnitTests.Fixtures;

public class TestFixture : IDisposable
{
    public AppDbContext Context { get; set; }
    public ITodoRepository TodoRepository { get; set; }

    public TestFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        Context = new AppDbContext(options);
        TodoRepository = new TodoRepository(Context);
    }
    public void Dispose()
    {
        Context.Dispose();
    }
}

public class TestDataFactory
{
    public static ListItem CreateListItem(string userId = "user1", string title = "testtask")
    {
        return new ListItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            IsCompleted = false,
            Description = "testdescription",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(string userId = "user1", string password = "user123!")
        {
            return new User
            {
                Id = userId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Name = $"user_{userId}",
                Email = $"user_{userId}@example.com",
                CreatedAt = DateTime.UtcNow
            };
        }


    public static List<ListItem> CreateTestTasks(int count, string userId = "user1")
    {
        return Enumerable.Range(1, count).Select(i => new ListItem
        {
            Id = Guid.NewGuid(),
            Title = $"Task{i}",
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();
    }

}

