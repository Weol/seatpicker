using System.Text.Json;
using Application.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters;

namespace Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, ApplicationConfiguration config)
    {
        return services
            .AddSingleton(new JsonSerializerOptions())
            .AddLoginService(options => options.Greeting = config.UserLoginGreetingMessage);
    }
}