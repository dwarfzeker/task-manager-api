using todoApp.Core.Interfaces;
using FluentAssertions;
using Moq;
using todoApp.Core.Services;

namespace todoApp.UnitTests;

public class PasswordServiceTests
{


    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }
    
    
    [Fact]
    public void HashPassword_ReturnsNotEmptyString()
    {
        var password = "testpassword1337";
        var hash = _passwordService.HashPassword(password);
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Theory]
    [InlineData("cutiepassword")]
    [InlineData("adminwithhat!205?")]
    [InlineData("longlongpasswordwithspecialsymbols$%#@!")]
    public void VerifyPassword_withCorrect_ShouldReturnTrue(string password)
    {
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(password, hash);
        result.Should().BeTrue();
    }
    [Theory]
    [InlineData("cutiepassword")]
    public void VerifyPassword_WrongPassword_ShouldReturnFalse(string password)
    {
        var incorrect = "incorrectPassword";
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(incorrect, hash);
        result.Should().BeFalse();
    }
    [Fact]
    public void HashPassword_SamePasswordTwice_SholdReturnDifferent()
    {
        var password = "testpassword1337";
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);
        hash1.Should().NotBe(hash2);
    }
    
    
    
}
