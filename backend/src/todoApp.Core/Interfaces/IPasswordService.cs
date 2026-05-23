namespace todoApp.Core.Interfaces;

public interface IPasswordService
{
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hashed);
    
}