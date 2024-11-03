using Seatpicker.Infrastructure.Entrypoints.Filters;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddHealthChecks();

        return services;
    }


    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapEntrypoints(builder =>
        {
            builder.AddEndpointFilter<FluentValidationFilter>();
            builder.AddEndpointFilter<HttpResponseExceptionFilter>();
        });

        return app;
    }
}