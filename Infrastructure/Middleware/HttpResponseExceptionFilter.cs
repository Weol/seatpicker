using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Seatpicker.Application;
using Seatpicker.Infrastructure.Adapters.DiscordClient;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Middleware;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is null) return;

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

        return null;
    }

    private ObjectResult HandleDomainException(DomainException e)
    {
        return new ObjectResult(e.Message)
        {
            StatusCode = 422,
        };
    }

    private static ObjectResult HandleModelValidationException(ModelValidationException e)
    {
        var errors = e.ValidationResultErrors.Select(x =>
            new
            {
                x.PropertyName,
                x.ErrorMessage,
                x.AttemptedValue
            });

        return new UnprocessableEntityObjectResult(errors);
    }

    private static ObjectResult HandleDiscordException()
    {
        return new ObjectResult("Discord API responded with non-successful status")
        {
            StatusCode = 500
        };
    }
}