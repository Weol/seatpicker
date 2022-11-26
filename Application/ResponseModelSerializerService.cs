using System.Text.Json;
using Application.Entrypoints;
using Application.Middleware;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Http;

namespace Application;

public interface IResponseModelSerializerService
{
    Task<HttpResponseData> Serialize<TModel>(HttpResponseData createResponse, TModel loginResponseModel);
}

public class ResponseModelSerializerService : IResponseModelSerializerService
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public ResponseModelSerializerService(JsonSerializerOptions jsonSerializerOptions)
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task<HttpResponseData> Serialize<TModel>(HttpResponseData response, TModel loginResponseModel)
    {
        await JsonSerializer.SerializeAsync(response.Body, loginResponseModel, jsonSerializerOptions);
        return response;
    }
}