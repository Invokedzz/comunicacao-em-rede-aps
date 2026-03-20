using System.Text.Json.Serialization;
using ComunicacaoEmRedesApi.Domain.Enums;

namespace ComunicacaoEmRedesApi.Domain.Results;

public class Result<T> 
{
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Value { get; }
    
    [JsonPropertyName("error_type")]
    public ErrorType ErrorType { get; }
    
    [JsonPropertyName("errors")]
    public List<Error> Errors { get; }
    
    private Result(bool isSuccess, T? value, ErrorType errorType, List<Error> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorType = errorType;
        Errors = errors;
    }

    public static Result<T> Success(T? value) => new(true, value, ErrorType.NoError, []);
    public static Result<T> Failure(ErrorType type, List<Error> errors) => new(false, default, type, errors);
}