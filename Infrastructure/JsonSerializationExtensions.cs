using System.Text.Json;
using System.Text.Json.Serialization;

namespace Seatpicker.Infrastructure;

public static class JsonSerializationExtensions
{
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters ={
                new JsonStringEnumConverter(),
            }
        };

        return services.AddSingleton(jsonSerializerOptions);
    }
}