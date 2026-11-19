using Microsoft.AspNetCore.Http.HttpResults;

namespace RoomHub.Api.Features.Shared;

public static class ErrorHttpMapping
{
    private static readonly IReadOnlyDictionary<ErrorType, int> StatusCodeByType = new Dictionary<ErrorType, int>
    {
        [ErrorType.Validation] = StatusCodes.Status400BadRequest,
        [ErrorType.NotFound] = StatusCodes.Status404NotFound,
        [ErrorType.Conflict] = StatusCodes.Status409Conflict
    };

    public static ProblemHttpResult ToProblem(this Error error) => TypedResults.Problem(
        title: error.Code,
        detail: error.Message,
        statusCode: StatusCodeByType[error.Type]);
}
