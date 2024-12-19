using FluentValidation;
using Seatpicker.Infrastructure.Entrypoints.Filters;
using Seatpicker.Infrastructure.Entrypoints.Middleware;
using Seatpicker.Infrastructure.Entrypoints.SignalR;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddValidatorsFromAssemblyContaining<AssemblyAnchor>()
            .AddSignalREntrypoints()
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddHealthChecks();

        return services;
    }

    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseTransactionMiddleware();
        app.UseSignalREntrypoints();
        app.MapEntrypoints(builder =>
        {
            builder.AddEndpointFilter<FluentValidationFilter>();
            builder.AddEndpointFilter<HttpResponseExceptionFilter>();
        });

        return app;
    }
}

public class AssemblyAnchor;