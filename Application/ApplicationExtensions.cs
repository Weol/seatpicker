using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Seatpicker.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        return services
            .AddSingleton(jsonSerializerOptions)
            .AddSingleton<IResponseModelSerializerService, ResponseModelSerializerService>()
            .AddSingleton<IRequestModelDeserializerService, RequestModelDeserializerService>();
    }
}