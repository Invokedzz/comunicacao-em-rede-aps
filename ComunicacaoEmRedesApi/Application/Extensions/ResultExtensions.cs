using ComunicacaoEmRedesApi.Domain.Enums;
using ComunicacaoEmRedesApi.Domain.Results;

namespace ComunicacaoEmRedesApi.Application.Extensions;

public static class ResultExtensions
{
    public static IResult ToResultFormat<T>(this IResultExtensions target, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result);
        }

        return result.ErrorType switch
        {
            ErrorType.BadRequest => Results.BadRequest(result),
            ErrorType.Conflict => Results.Conflict(result),
            ErrorType.NotFound => Results.NotFound(result),
            _ => Results.Problem()
        };
    }
}