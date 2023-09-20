using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure;

public static class JsonSerializationExtensions
{
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
    {
        var jsonOptions = new JsonOptions();
        ConfigureJsonOptions(jsonOptions.JsonSerializerOptions);

        return services
            .AddSingleton(jsonOptions.JsonSerializerOptions)
            .Configure<JsonOptions>(options => ConfigureJsonOptions(options.JsonSerializerOptions));
    }

    public static void ConfigureJsonOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}