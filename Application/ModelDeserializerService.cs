using System.Text.Json;
using Application.Middleware;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Http;

namespace Application;

public interface IModelDeserializerService
{
    Task<TModel> Deserialize<TModel, TValidator>(HttpRequestData request) where TValidator : IValidator<TModel>, new();
}

public class ModelDeserializerService : IModelDeserializerService
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public ModelDeserializerService(JsonSerializerOptions jsonSerializerOptions)
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