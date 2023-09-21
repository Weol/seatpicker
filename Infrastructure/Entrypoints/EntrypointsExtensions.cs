using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddLoggedInUserAccessor()
            .AddModelValidator()
            .AddHealthChecks()
            .Services
            .AddControllers(ConfigureMvcOptions);

        return services;
    }

    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseMiddleware<RequestScopedAggregateTransactionMiddleware>();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.MapControllers();

        return app;
    }

    private static void ConfigureMvcOptions(MvcOptions options)
    {
         options.Filters.Add<HttpResponseExceptionFilter>();
    }
}