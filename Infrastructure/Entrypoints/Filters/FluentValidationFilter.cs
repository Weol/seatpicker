using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Seatpicker.Infrastructure.Entrypoints.Filters;

public class FluentValidationFilter : IAsyncActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    private readonly IServiceProvider provider;

    public FluentValidationFilter(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var (_, value) in context.ActionArguments)
        {
            if (value is null) continue;

            var service = provider.GetService(typeof(IValidator<>).MakeGenericType(value.GetType()));

            if (service is not IValidator validator) continue;

            var validationResult = await validator.ValidateAsync(new ValidationContext<object>(value), context.HttpContext.RequestAborted);
            if (!validationResult.IsValid)
            {
                throw new ModelValidationException(validationResult.Errors);
            }
        }

        await next();
    }

    public class ModelValidationException : Exception
    {
        public List<ValidationFailure> ValidationResultErrors { get; }

        public ModelValidationException(List<ValidationFailure> validationResultErrors)
        {
            ValidationResultErrors = validationResultErrors;
        }
    }
}