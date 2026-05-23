using System.Net;
using todoApp.Core.DTOs;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions;
using System;
namespace todoApp.UnitTests.IntegrationTests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public BasicTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task GetTasksWithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAndLogin_ShouldWork()
    {
        var registerDto = new RegisterDTO
        {
            UserName = $"testuser_{Guid.NewGuid()}",
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test123!"
        };
        
        _output.WriteLine($"Testing with user: {registerDto.UserName}");
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        
        if (registerResponse.StatusCode != HttpStatusCode.OK)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Register error: {error}");
        }
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginDto = new LoginDTO
        {
            Login = registerDto.UserName,
            Password = registerDto.Password
        };
        
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            var error = await loginResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Login error: {error}");
        }
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}