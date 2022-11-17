using System.Text.Json;
using Application.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationExtensions
{
    private static IConfiguration Configuration { get; set; }
    
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        Configuration = config;

        return services
            .AddSingleton(new JsonSerializerOptions())
            .AddLoginJwtService(ConfigureLoginJwtService);
    }

    private static void ConfigureLoginJwtService(LoginJwtService.Options options)
    {
        options.ClientId = Configuration["ClientId"];
    }
}