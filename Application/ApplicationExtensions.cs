using System.Text.Json;
using Application.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddSingleton(new JsonSerializerOptions())
            .AddLoginService();
    }
}