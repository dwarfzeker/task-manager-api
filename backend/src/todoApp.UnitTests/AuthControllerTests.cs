using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;
using Moq;
using todoApp.Core.DTOs;
using todoApp.Web.Controllers;
using todoApp.Core.Interfaces;
using todoApp.Core.Entities;

namespace todoApp.UnitTests;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly AuthController _controller;
    private readonly Mock<IPasswordService> _passwordMock;
    private readonly Mock<IJwtService> _jwtMock;

    public AuthControllerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _passwordMock = new Mock<IPasswordService>();
        _controller = new AuthController(_repositoryMock.Object, _passwordMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task Register_ValidUser_ShouldReturnOk()
    {
        var dto = new RegisterDTO
        {
            Email = "example@example.com",
            UserName = "user1337",
            Password = "userspassword"
        };
        var hashedPassword = "hashed_password_123";
        var generatedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var userId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    
        User createdUser = new User();
        _repositoryMock
            .Setup(r => r.UsernameExistsAsync(dto.UserName))
            .ReturnsAsync(false);
    
        _passwordMock
            .Setup(p => p.HashPassword(dto.Password))
            .Returns(hashedPassword);
    
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => 
            {
                createdUser = user;
                user.Id = userId; 
            });
    
        _jwtMock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(generatedToken);

        var result = await _controller.Register(dto);
        var okResult = result.Should().BeAssignableTo<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponseDTO>().Subject;
        response.Token.Should().Be(generatedToken);
        response.UserName.Should().Be(dto.UserName);
        response.UserId.Should().Be(userId);
        
        var cookies = httpContext.Response.Headers["Set-Cookie"].ToString();
        cookies.Should().Contain("AccessToken");
        cookies.Should().Contain(generatedToken);
        _repositoryMock.Verify(r => r.UsernameExistsAsync(dto.UserName), Times.Once);
        _passwordMock.Verify(p => p.HashPassword(dto.Password), Times.Once);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => 
            u.Name == dto.UserName && 
            u.Email == dto.Email && 
            u.PasswordHash == hashedPassword
        )), Times.Once);
        _jwtMock.Verify(j => j.GenerateToken(It.Is<User>(u => 
            u.Name == dto.UserName && 
            u.Email == dto.Email
        )), Times.Once);
        
    }

    [Fact]
    public async Task Register_UserExists_ShouldReturnBadRequest()
    {
        var dto = new RegisterDTO
        {
            Email = "example@example.com",
            UserName = "user1337",
            Password = "userspassword"
        };
        var user = new User
        {
            Name = "user1337",
            Email = "example2@example.com",
            PasswordHash = "hashed_password_123"
        };
        await _repositoryMock.Object.CreateAsync(user);
        
        _repositoryMock.Setup(r => r.UsernameExistsAsync(dto.UserName)).ReturnsAsync(true);
        var result = await _controller.Register(dto);
        _repositoryMock.Verify(r => r.UsernameExistsAsync(dto.UserName), Times.Once);
        result.Should().BeAssignableTo<BadRequestObjectResult>();
        
    }
    
    [Fact]
    public async Task Login_WithNullUser_ShouldReturnUnauthorized()
    {
        var login = "user232";
        var dto = new LoginDTO
        {
            Login = login,
            Password = "password232"
        };
        _repositoryMock.Setup(a => a.GetUserByNameAsync(login)).ReturnsAsync((User)null);
        var result = await _controller.Login(dto);
        var unauthorized = result.Should().BeAssignableTo<UnauthorizedObjectResult>().Subject;
        _repositoryMock.Verify(a => a.GetUserByNameAsync(login), Times.Once);
    }
    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        var login = "user232";
        var password = "password232";
        var hash = _passwordMock.Object.HashPassword(password);
        var dto = new LoginDTO
        {
            Login = login,
            Password = "wrongpassword232"
        };
        var user = new User
        {
            Name = login,
            PasswordHash = hash,
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Today.AddDays(-1)
        };
        _repositoryMock.Object.CreateAsync(user);
        _repositoryMock.Setup(a => a.GetUserByNameAsync(login)).ReturnsAsync(user);
        _passwordMock.Setup(a => a.VerifyPassword(dto.Password, hash)).Returns(false);
        var result = await _controller.Login(dto);
        var unauthorized = result.Should().BeAssignableTo<UnauthorizedObjectResult>().Subject;
        _repositoryMock.Verify(a => a.GetUserByNameAsync(login), Times.Once);
        _passwordMock.Verify(a => a.VerifyPassword(dto.Password, hash), Times.Once);
    }
    [Fact]
    public void Logout_ShouldDeleteAccessToken_AndReturnOk()
    {
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        httpContext.Request.Headers["Cookie"] = "AccessToken=some_token_value";
        
        var result = _controller.Logout();
        var okResult = result.Should().BeAssignableTo<OkObjectResult>().Subject;
        Assert.Equal("Logged out successfully",  okResult.Value);
        var cookieHeaders = httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("AccessToken", cookieHeaders);
        Assert.Contains("expires=", cookieHeaders);
        Assert.Contains("AccessToken=;", cookieHeaders);
    }
    
    [Fact]
    public void Logout_ShouldAlwaysReturnOk_EvenWithoutCookie()
    {
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var result = _controller.Logout();
    
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Logged out successfully", okResult.Value);
    
        var cookieHeaders = httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("AccessToken", cookieHeaders);
    }
    [Fact]
    public void Me_WithAuthenticatedUser_ShouldReturnUserName()
    {
        var userName = "testuser1337";
    
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
    
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };
    
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    
        var result = _controller.Me();
    
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    
        Assert.Equal(userName, dict["UserName"].ToString());
    }
    [Fact]
    public void Me_WithUnauthenticatedUser_ShouldReturnNull()
    {
        // Arrange
        // User = null (неавторизованный)
        var httpContext = new DefaultHttpContext
        {
            User = null
        };
    
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    
        // Act
        var result = _controller.Me();
    
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    
        Assert.Null(dict?["UserName"]);

    }
    
}