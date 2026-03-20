using ComunicacaoEmRedesApi.Application.Dtos;
using ComunicacaoEmRedesApi.Domain.Enums;
using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Domain.Repositories;
using ComunicacaoEmRedesApi.Domain.Results;
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
    
    public async Task<Result<RegisterResponseDto>> Register(RegisterRequestDto request)
    {
        var domainErrors = SessionDomainValidations.GetRegisterErrors(request.Email, request.Password);

        if (domainErrors.Count > 0)
        {
            return Result<RegisterResponseDto>.Failure(ErrorType.BadRequest, domainErrors);
        }

        if (await DoesRequestedEmailAlreadyExists(request.Email))
        {
            var conflictError = new Error { Code = Error.Codes.EmailAlreadyExists, Message = "This email already exists!" };
            return Result<RegisterResponseDto>.Failure(ErrorType.Conflict, [conflictError]);
        }
        
        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.Password
        };

        user.PasswordHash = HashUserPassword(user);
        
        await _userRepository.SaveUserAsync(user);
        var response = RegisterResponseDto.Get(user.Email);

        return Result<RegisterResponseDto>.Success(response);
    }

    private async Task<bool> DoesRequestedEmailAlreadyExists(string email)
    {
        return await _userRepository.DoesEmailExists(email);
    }
    
    private string HashUserPassword(User user)
    {
        return _encryption.HashPassword(user, user.PasswordHash);
    }
    
    private static class SessionDomainValidations
    {
        public static List<Error> GetRegisterErrors(string email, string password)
        {
            var errors = new List<Error>();

            if (!IsEmailDomainValid(email))
            {
                errors.Add(new Error
                {
                    Code = Error.Codes.InvalidEmail, Message = "Email domain is not valid!"
                });
            }

            if (!IsPasswordLengthCorrect(password))
            {
                errors.Add(new Error
                {
                    Code = Error.Codes.InvalidPassword, Message = "Password length must be between 8 and 15 characters!"
                });
            }

            if (!IsPasswordStructureValid(password))
            {
                errors.Add(new Error
                {
                    Code = Error.Codes.InvalidPassword, Message = "Password must contain letters, numbers and a special character!"
                });
            }

            return errors;
        }
        
        private static bool IsEmailDomainValid(string email)
        {
            var providers = Enum.GetNames<AvailableEmailProviders>().Select(e => e.ToLower());
            return DoesEmailEndsWithDomain(email, providers);
        }

        private static bool DoesEmailEndsWithDomain(string email, IEnumerable<string> providers) =>
            providers.Any(provider => email.EndsWith(provider + AvailableDomains.DotCom) || email.EndsWith(provider + AvailableDomains.DotComBr));

        private static bool IsPasswordLengthCorrect(string password)
        {
            return password.Length is >= 8 and <= 15;
        }
        
        private static bool IsPasswordStructureValid(string password)
        {
            bool hasLetter = false, hasNumber = false, hasSpecialChar = false;
            const string allowedSpecialChars = "._-@!?,:;()";
            
            foreach (var digit in password)
            {
                if (char.IsLetter(digit)) hasLetter = true;
                else if (char.IsNumber(digit)) hasNumber = true;
                else if (allowedSpecialChars.Contains(digit)) hasSpecialChar = true;
                else return false;
            }
            
            return hasLetter && hasNumber && hasSpecialChar;
        }
        
        private struct AvailableDomains
        {
            public const string DotCom = ".com";
            public const string DotComBr = ".com.br";
        }
    }
}