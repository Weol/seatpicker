﻿using Seatpicker.Infrastructure.Entrypoints.GraphQL;
using Seatpicker.Infrastructure.Entrypoints.Http;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGraphQLEntrypoints()
            .AddEndpointsApiExplorer()
            .AddLoggedInUserAccessor()
            .AddHealthChecks();

        return services;
    }


    public static WebApplication UseEntrypoints(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapGraphQLEntrypoints();
        app.MapEntrypoints();

        return app;
    }
}