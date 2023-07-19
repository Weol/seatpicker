using FluentValidation;
using FluentValidation.Results;

namespace Seatpicker.Infrastructure.Utils;

public interface IValidateModel
{
    Task Validate<TModel, TModelValidator>(TModel model)
        where TModelValidator : IValidator<TModel>, new();
}

public class ValidateModel : IValidateModel
{
    public async Task Validate<TModel, TModelValidator>(TModel model) where TModelValidator : IValidator<TModel>, new()
    {
        var validator = new TModelValidator();
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }
    }
}

public class ModelValidationException : Exception
{
    public List<ValidationFailure> ValidationResultErrors { get; }

    public ModelValidationException(List<ValidationFailure> validationResultErrors)
    {
        ValidationResultErrors = validationResultErrors;
    }
}

public static class ModelValidatorExtensions
{
    public static IServiceCollection AddModelValidator(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IValidateModel, ValidateModel>();
    }
}