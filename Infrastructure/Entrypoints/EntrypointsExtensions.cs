using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddHealthChecks()
            .Services
            .AddFluentValidation()
            .AddControllers(ConfigureMvcOptions);

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssemblyContaining<Program>()
            .Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
    }

    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapEntrypoints();

        return app;
    }

    private static void ConfigureMvcOptions(MvcOptions options)
    {
         options.Filters.Add<HttpResponseExceptionFilter>();
         options.Filters.Add<FluentValidationFilter>();
    }
}