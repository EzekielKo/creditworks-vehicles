using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Vehicles.Application;

namespace Vehicles.Api;

/// <summary>Maps exceptions to consistent ProblemDetails responses without leaking internals.</summary>
public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case ValidationFailedException validation:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed.",
                };
                problem.Extensions["errors"] = validation.Errors;
                break;

            case NotFoundException notFound:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found.",
                    Detail = notFound.Message,
                };
                break;

            default:
                logger.LogError(exception, "Unhandled exception");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                };
                break;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
