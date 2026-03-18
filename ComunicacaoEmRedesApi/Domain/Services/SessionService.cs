using System.Text.RegularExpressions;
using ComunicacaoEmRedesApi.Application.Dtos;
using ComunicacaoEmRedesApi.Domain.Enums;
using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Domain.Repositories;
using ComunicacaoEmRedesApi.Domain.Services.Interfaces;
using ComunicacaoEmRedesApi.Infrastructure.Security.Interfaces;

namespace ComunicacaoEmRedesApi.Domain.Services;

public class SessionService : ISessionService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordEncryption _encryption;

    public SessionService(IUserRepository userRepository, IPasswordEncryption encryption)
    {
        _userRepository = userRepository;
        _encryption = encryption;
    }
    
    public async Task Register(RegisterRequestDto request)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.Password
        };

        var hash = _encryption.HashPassword(user, user.PasswordHash);
        Console.WriteLine(hash);
        
        var isValid = SessionDomainValidations.IsEmailDomainValid(user.Email);
        if (isValid)
        {
            Console.WriteLine("Nice one! User saved!");
            await _userRepository.SaveUserAsync(user);
            return;
        }
        
        Console.WriteLine("Oops! Invalid email!");
    }

    private static class SessionDomainValidations
    {
        public static bool IsEmailDomainValid(string email)
        {
            var providers = Enum.GetNames<AvailableEmailProviders>().Select(e => e.ToLower());
            return DoesEmailEndsWithDomain(email, providers);
        }

        public static bool IsPasswordStructureValid(string password)
        {
            return SessionDomainRegex.PasswordDefaultPattern.IsMatch(password);
        }

        private static bool DoesEmailEndsWithDomain(string email, IEnumerable<string> providers) =>
            providers.Any(provider => email.EndsWith(provider + AvailableDomains.DotCom) || email.EndsWith(provider + AvailableDomains.DotComBr));

        private struct AvailableDomains
        {
            public const string DotCom = ".com";
            public const string DotComBr = ".com.br";
        }
    }

    private static class SessionDomainRegex
    {
        private const int OneSecond = 1000;
        private static readonly TimeSpan Interval = new(OneSecond);
        
        public static readonly Regex PasswordDefaultPattern 
            = new(AvailableRegex.AllowLettersNumbersAndSomeSpecialChars, RegexOptions.Compiled, Interval);

        private struct AvailableRegex
        {
            public const string AllowLettersNumbersAndSomeSpecialChars = "^[a-zA-Z0-9 _.\\-@!?,:;()]+$";
        }
    }
}