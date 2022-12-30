using System.Net;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Seatpicker.Application.Middleware;

public class ModelValidationExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AggregateException e)
        {
            if (e.InnerException is not ModelValidationException validationException) throw;
            
            var errors = validationException.ValidationResult.Errors.Select(error => new
            {
                Property = error.PropertyName,
                Value = error.AttemptedValue,
                Error = error.ErrorMessage,
            });
            
            var requestData = await context.GetHttpRequestDataAsync();
            if (requestData is not null)
            {
                var response = requestData.CreateResponse(HttpStatusCode.BadRequest);
                
                await response.WriteAsJsonAsync(errors, response.StatusCode);

                var invocationResult = context.GetInvocationResult();
                invocationResult.Value = response;
            }
        }
    }
}

public class ModelValidationException : Exception
{
    public ValidationResult ValidationResult { get; }
    
    public ModelValidationException(ValidationResult validationResult)
    {
        ValidationResult = validationResult;
    }
}