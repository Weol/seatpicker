using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

namespace Seatpicker.Infrastructure;

public static class JsonSerializationExtensions
{
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
    {
        var jsonOptions = new JsonOptions();
        ConfigureJsonOptions(jsonOptions.SerializerOptions);

        return services
            .AddSingleton(jsonOptions.SerializerOptions)
            .Configure<JsonOptions>(options => ConfigureJsonOptions(options.SerializerOptions));
    }

    public static void ConfigureJsonOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}