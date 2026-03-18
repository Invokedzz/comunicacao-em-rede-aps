using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Infrastructure.Security.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ComunicacaoEmRedesApi.Infrastructure.Security;

public class PasswordEncryption : IPasswordEncryption
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordEncryption(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }
    
    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string password)
    {
        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return passwordVerification == PasswordVerificationResult.Success;
    }
}