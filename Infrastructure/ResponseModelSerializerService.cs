using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Seatpicker.Infrastructure;

public interface IResponseModelSerializerService
{
    Task<string> Serialize<TModel>(TModel loginResponseModel);
}

public class ResponseModelSerializerService : IResponseModelSerializerService
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public ResponseModelSerializerService(JsonSerializerOptions jsonSerializerOptions)
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    public Task<string> Serialize<TModel>(TModel loginResponseModel)
    {
        return Task.FromResult(JsonSerializer.Serialize(loginResponseModel, jsonSerializerOptions));
    }
}