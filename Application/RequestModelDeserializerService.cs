using System.Text.Json;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Http;
using Seatpicker.Application.Middleware;

namespace Seatpicker.Application;

public interface IRequestModelDeserializerService
{
    Task<TModel> Deserialize<TModel, TValidator>(HttpRequestData request) where TValidator : IValidator<TModel>, new();
}

public class RequestModelDeserializerService : IRequestModelDeserializerService
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public RequestModelDeserializerService(JsonSerializerOptions jsonSerializerOptions)
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task<TModel> Deserialize<TModel, TValidator>(HttpRequestData request) where TValidator : IValidator<TModel>, new()
    {
        var validator = new TValidator();
        
        var body = await request.ReadAsStringAsync() ?? throw new NullReferenceException();
        var model = JsonSerializer.Deserialize<TModel>(body, jsonSerializerOptions) ?? throw new NullReferenceException();
        
        var result = await validator.ValidateAsync(model);
        if (!result.IsValid) throw new ModelValidationException(result);
        
        return model;
    }
}