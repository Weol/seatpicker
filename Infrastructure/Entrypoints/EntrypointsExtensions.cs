using FluentValidation;
using FluentValidation.AspNetCore;
using Seatpicker.Infrastructure.Entrypoints.Filters;
using Seatpicker.Infrastructure.Entrypoints.Http.Frontend;
using Seatpicker.Infrastructure.Entrypoints.Middleware;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddValidatorsFromAssemblyContaining<AssemblyAnchor>()
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddHealthChecks();

        return services;
    }


    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseTransactionMiddleware();
        app.MapEntrypoints(builder =>
        {
            builder.AddEndpointFilter<FluentValidationFilter>();
            builder.AddEndpointFilter<HttpResponseExceptionFilter>();
        });

        return app;
    }
}

public class AssemblyAnchor;