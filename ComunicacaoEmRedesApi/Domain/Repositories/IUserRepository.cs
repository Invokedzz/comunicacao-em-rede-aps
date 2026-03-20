using ComunicacaoEmRedesApi.Domain.Models;
using LanguageExt;

namespace ComunicacaoEmRedesApi.Domain.Repositories;

public interface IUserRepository
{
    Task SaveUserAsync(User user);
    Task<bool> DoesEmailExists(string email);
    Task<Option<User>> GetUserByIdAsync(Guid id);
}