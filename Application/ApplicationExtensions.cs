using Application.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters;

namespace Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, ApplicationConfiguration config)
    {
        return services.AddLoginService(options => options.Greeting = config.UserLoginGreetingMessage);
    }
}