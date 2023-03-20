using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure;

public static class JsonSerializationExtensions
{
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
    {
        var jsonOptions = new JsonOptions();
        ConfigureJsonOptions(jsonOptions);

        return services
            .AddSingleton(jsonOptions.JsonSerializerOptions)
            .Configure<JsonOptions>(ConfigureJsonOptions);
    }

    public static void ConfigureJsonOptions(JsonOptions jsonOptions)
    {
        jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    }
}