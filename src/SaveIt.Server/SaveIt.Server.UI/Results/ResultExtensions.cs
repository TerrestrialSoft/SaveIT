using FluentResults;
using SaveIt.Server.UI.Results.Errors;
using System.Net;

namespace SaveIt.Server.UI.Results;

public static class ResultExtensions
{
    public static Result ToResult(this HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode)
        {
            return Result.Ok();
        }

        return message.StatusCode switch
        {
            HttpStatusCode.BadRequest => Result.Fail(ApiErrors.General()),
            HttpStatusCode.Unauthorized => Result.Fail(ApiErrors.UnAuthorized()),
            HttpStatusCode.Forbidden => Result.Fail(ApiErrors.Forbidden()),
            HttpStatusCode.NotFound => Result.Fail(ApiErrors.NotFound()),
            HttpStatusCode.InternalServerError => Result.Fail(ApiErrors.InternalServer()),
            _ => Result.Fail("Unknown error."),
        };
    }

    public static IResult ToAspResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Microsoft.AspNetCore.Http.Results.Ok(result.Value)
            : result.ToResult().ToAspResult();
    }

    public static IResult ToAspResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.NoContent();
        }
        else if (result.HasError<ApiErrors.NotFoundError>(out var e1))
        {
            return Microsoft.AspNetCore.Http.Results.NotFound(e1.ElementAt(0).Message);
        }
        else if (result.HasError<ApiErrors.ForbiddenError>())
        {
            return Microsoft.AspNetCore.Http.Results.Forbid();
        }
        else if (result.HasError<ApiErrors.UnAuthorizedError>())
        {
            return Microsoft.AspNetCore.Http.Results.Unauthorized();
            
        }
        else if (result.HasError<ApiErrors.InternalServerError>())
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
        else
        {
            return Microsoft.AspNetCore.Http.Results.BadRequest();
        }
    }
}
