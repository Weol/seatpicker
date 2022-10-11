using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Application.Middleware;

public class JsonExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AggregateException aggregateException) when (aggregateException.InnerException is JsonException e)
        {
            var requestData = await context.GetHttpRequestDataAsync();
            if (requestData is not null)
            {
                var response = requestData.CreateResponse(HttpStatusCode.BadRequest);
                
                await response.WriteAsJsonAsync(e.Message, response.StatusCode);

                var invocationResult = context.GetInvocationResult();
                invocationResult.Value = response;
            }
        }
    }
}