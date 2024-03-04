using FluentValidation;
using FluentValidation.Results;

namespace Seatpicker.Infrastructure.Entrypoints.Filters;

public class FluentValidationFilter : IEndpointFilter
{
    private readonly IServiceProvider provider;

    public FluentValidationFilter(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var value in context.Arguments)
        {
            if (value is null) continue;

            var service = provider.GetService(typeof(IValidator<>).MakeGenericType(value.GetType()));

            if (service is not IValidator validator) continue;

            var validationResult = await validator.ValidateAsync(new ValidationContext<object>(value),
                context.HttpContext.RequestAborted);
            
            if (validationResult.IsValid) continue;
            
            var errors = validationResult.Errors.Select(x =>
                new
                {
                    x.PropertyName,
                    x.ErrorMessage,
                    x.AttemptedValue,
                });

            return Results.BadRequest(errors);
        }

        return await next(context);
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