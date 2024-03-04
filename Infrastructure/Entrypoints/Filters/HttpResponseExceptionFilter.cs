using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http;

namespace Seatpicker.Infrastructure.Entrypoints.Filters;

public class HttpResponseExceptionFilter : IEndpointFilter
{
    private readonly ILogger<HttpResponseExceptionFilter> logger;

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        try
        {
            return await next(context);
        }
        catch (DomainException domainException)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogError(domainException, "{Message}", domainException.Message);
            
            return HandleDomainOrApplicationException(domainException);
        }
        catch (Application.ApplicationException applicationException)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogError(applicationException, "{Message}", applicationException.Message);
            
            return HandleDomainOrApplicationException(applicationException);
        }
        catch (BadRequestException badRequestException)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogError(badRequestException, "{Message}", badRequestException.Message);
            
            return HandleBadRequestException(badRequestException);
        }
    }

    private static IResult HandleDomainOrApplicationException(Exception e)
    {
        var exceptionName = e.GetType().Name;

        var props = e.GetType()
            .GetProperties()
            .Where(prop => prop.DeclaringType == e.GetType());

        var response = new Dictionary<string, object>();
        foreach (var property in props)
        {
            var value = property.GetValue(e);
            var name = property.Name;

            if (value is not null) response[name] = value;
        }

        var statusCode = 422;
        if (exceptionName.Contains("NotFound")) statusCode = 404;
        if (exceptionName.Contains("Already")) statusCode = 409;
        if (exceptionName.Contains("Conflict")) statusCode = 409;

        return statusCode switch
        {
            422 => Results.UnprocessableEntity(response),
            404 => Results.NotFound(response),
            409 => Results.Conflict(response),
            
            // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
            _ => Results.Problem()
        };
    }

    private static IResult HandleBadRequestException(BadRequestException badRequestException)
    {
        return Results.BadRequest(new
        {
            badRequestException.Message,
        });
    }
}