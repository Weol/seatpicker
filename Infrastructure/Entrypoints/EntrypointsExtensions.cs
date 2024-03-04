using FluentValidation;
using Seatpicker.Infrastructure.Entrypoints.Filters;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddFluentValidation()
            .AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssemblyContaining<Program>();
    }

    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseDeveloperExceptionPage();
        app.MapEntrypoints(builder =>
        {
            builder.AddEndpointFilter<FluentValidationFilter>();
            builder.AddEndpointFilter<HttpResponseExceptionFilter>();
        });

        return app;
    }
}