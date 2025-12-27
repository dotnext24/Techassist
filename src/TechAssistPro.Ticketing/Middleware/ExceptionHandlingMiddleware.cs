using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.SharedKernel.Exceptions;
using TechAssistPro.SharedKernel.Responses;

namespace TechAssistPro.Ticketing.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IResponseFactory responder)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "NotFoundException exception for {Path} | CorrelationId={CorrelationId}", context.Request.Path, CorrelationContext.CorrelationId);


            var body = responder.Error(new
            {
                message = ex.Message,
                correlationId = CorrelationContext.CorrelationId
            });
            var result = Results.NotFound(body);
            context.Response.ContentType = "application/json";
            await result.ExecuteAsync(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogError(ex, "Validation error for {Path} | CorrelationId={CorrelationId}", context.Request.Path, CorrelationContext.CorrelationId);
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var body = responder.Error(errors);

            var result = Results.BadRequest(body);

            context.Response.ContentType = "application/json";
            await result.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path} | CorrelationId={CorrelationId}", context.Request.Path, CorrelationContext.CorrelationId);


            var body = responder.Error(new
            {
                message = "An unexpected error occurred.",
                correlationId = CorrelationContext.CorrelationId
            });
            var result = Results.Json(data: body, statusCode: StatusCodes.Status500InternalServerError);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await result.ExecuteAsync(context);
        }
    }
}
