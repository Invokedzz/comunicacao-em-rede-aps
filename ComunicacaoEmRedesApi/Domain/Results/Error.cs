using System.Text.Json.Serialization;

namespace ComunicacaoEmRedesApi.Domain.Results;

public class Error
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }
    
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    public struct Codes
    {
        public const string InvalidEmail = "INVALID_EMAIL";
        public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
        public const string InvalidPassword = "INVALID_PASSWORD";
    }
}