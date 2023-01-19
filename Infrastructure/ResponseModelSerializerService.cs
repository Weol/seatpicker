using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Seatpicker.Host;

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