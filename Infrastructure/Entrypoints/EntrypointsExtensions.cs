using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Seatpicker.Infrastructure.Middleware;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddModelValidator()
            .AddBus(GetMassTransitOptions(configuration))
            .AddHealthChecks()
            .Services.AddControllers(ConfigureMvcOptions);

        return services;
    }

    private static void ConfigureMvcOptions(MvcOptions options)
    {
        options.Filters.Add<HttpResponseExceptionFilter>();
    }

    private static MassTransitOptions GetMassTransitOptions(IConfiguration configuration)
    {
        var options = new MassTransitOptions();
        configuration.GetSection("MassTransit").Bind(options);
        return options;
    }
}