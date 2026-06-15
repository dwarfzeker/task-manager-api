using Xunit;
using FluentAssertions;
using Moq;
using todoApp.Core.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using todoApp.Core.DTOs;
using todoApp.Web.Controllers;

namespace todoApp.UnitTests;

public class ProfileControllerTests
{
    private readonly ProfileController _controller;
    private readonly Mock<IStatsService> _statsServiceMock;

    public ProfileControllerTests()
    {
        _statsServiceMock = new Mock<IStatsService>();
        _controller = new ProfileController(_statsServiceMock.Object);
        SetupControllerContext(_controller, "user1337");
    }
    
    private void SetupControllerContext(ProfileController controller, string userId)
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
    public async Task GetProfile_EmptyProfile_ShouldReturnOk()
    {
        _statsServiceMock.Setup(a => a.GetUserStats("user1337"));
        var result = await _controller.GetProfile();
        _statsServiceMock.Verify(a => a.GetUserStats("user1337"), Times.Once);
        var okResult = result.Should().BeAssignableTo<OkObjectResult>().Subject;
        var dto = okResult.Value;

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetProfile_ProfileWithData_ShouldReturnDto()
    {
        var userId = "user1337";
        var expectedStats = new UserStatsDTO
        {
            Name = "Vasily",
            CompletedTasks = 1,
            TotalTasksCount = 2,
            CurrentStreak = 1,
            LongestStreak = 1,
            LastActivity = DateTime.Today,
            CompletionRate = 50.0
        };
        
        _statsServiceMock
            .Setup(s => s.GetUserStats(userId))
            .ReturnsAsync(expectedStats);
        
        var result = await _controller.GetProfile();
        
        _statsServiceMock.Verify(s => s.GetUserStats(userId), Times.Once);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<UserStatsDTO>().Subject;
        
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Vasily");
        dto.CompletedTasks.Should().Be(1);
        dto.TotalTasksCount.Should().Be(2);
        dto.CompletionRate.Should().Be(50.0);
        dto.CurrentStreak.Should().Be(1);
        dto.LongestStreak.Should().Be(1);
    }

    [Fact]
    public async Task GetProfile_InvalidUser_ShouldThrowUnauthorized()
    {
        var controller = new ProfileController(_statsServiceMock.Object);
        var emptyClaims = new ClaimsIdentity(); 
        var principal = new ClaimsPrincipal(emptyClaims);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

         await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetProfile());
    }
}