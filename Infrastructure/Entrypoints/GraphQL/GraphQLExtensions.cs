using HotChocolate.Data;
using Seatpicker.Infrastructure.Entrypoints.GraphQL.Guild;

// ReSharper disable InconsistentNaming
namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQLEntrypoints(this IServiceCollection services)
    {
        services.AddGraphQLServer("guild")
            .AddQueryType<Guild.Queries>()
            .AddMutationType<Guild.Mutations>()
            .AddTypeExtension<GuildExtensions>()
            .AddTypeExtension<ProjectedLanExtensions>()
            .AddMartenFiltering()
            .AddMartenSorting();

        services.AddGraphQLServer("guildless")
            .AddQueryType<Guildless.Queries>()
            .AddMartenFiltering()
            .AddMartenSorting();

        return services;
    }

    public static IApplicationBuilder MapGraphQLEntrypoints(this IApplicationBuilder builder)
    {
        builder.MapGraphQL("/guild", "guild");
        builder.MapGraphQL("/guildless", "guildless");

        return builder;
    }
}