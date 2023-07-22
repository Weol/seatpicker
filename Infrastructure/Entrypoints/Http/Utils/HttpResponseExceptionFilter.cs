using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Utils;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public readonly ILogger<HttpResponseExceptionFilter> logger;

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
    {
        this.logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is null) return;

        logger.LogError(context.Exception, context.Exception.Message);

        var result = HandleException(context.Exception);
        if (result is not null)
        {
            context.Result = result;
            context.ExceptionHandled = true;
        }
    }

    private ObjectResult? HandleException(Exception exception)
    {
        if (exception is DomainException domainException)
        {
            return HandleDomainException(domainException);
        }

        if (exception is DiscordException)
        {
            return HandleDiscordException();
        }

        if (exception is ModelValidationException modelValidationException)
        {
            return HandleModelValidationException(modelValidationException);
        }

        if (exception is BadRequestException badRequestException)
        {
            return HandleBadRequestException(badRequestException);
        }

        return null;
    }

    private ObjectResult HandleDomainException(DomainException e)
    {
        var exceptionName = e.GetType()
            .Name;

        var statusCode = 422;
        if (exceptionName.Contains("NotFound")) statusCode = 404;
        if (exceptionName.Contains("Already")) statusCode = 409;
        if (exceptionName.Contains("Conflict")) statusCode = 409;

        var props = e.GetType()
            .GetProperties()
            .Where(prop => prop.DeclaringType == e.GetType());

        var response = new Dictionary<string, object>();
        foreach (var property in props)
        {
            var value = property.GetValue(e);
            var name = property.Name;

            response[name] = value;
        }

        return new ObjectResult(response)
        {
            StatusCode = statusCode,
        };
    }

    private ObjectResult HandleBadRequestException(BadRequestException badRequestException)
    {
        return new BadRequestObjectResult(new
        {
            badRequestException.Message,
        });
    }

    private static ObjectResult HandleModelValidationException(ModelValidationException e)
    {
        var errors = e.ValidationResultErrors.Select(x =>
            new
            {
                x.PropertyName,
                x.ErrorMessage,
                x.AttemptedValue,
            });

        return new BadRequestObjectResult(errors);
    }

    private static ObjectResult HandleDiscordException()
    {
        return new ObjectResult("Discord API responded with non-successful status")
        {
            StatusCode = 500
        };
    }
}