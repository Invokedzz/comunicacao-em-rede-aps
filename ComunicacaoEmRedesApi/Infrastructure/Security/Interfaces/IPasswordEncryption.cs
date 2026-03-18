using ComunicacaoEmRedesApi.Domain.Models;

namespace ComunicacaoEmRedesApi.Infrastructure.Security.Interfaces;

public interface IPasswordEncryption
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string password);
}