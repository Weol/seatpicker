using HotChocolate.Data;
using Seatpicker.Infrastructure.Entrypoints.GraphQL.GuildQueries;

// ReSharper disable InconsistentNaming
namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQLEntrypoint(this IServiceCollection services)
    {
        services.AddGraphQLServer("guild")
            .AddQueryType<GuildQueries.Queries>()
            .AddMutationType<GuildQueries.Mutations>()
            .AddTypeExtension<GuildExtensions>()
            .AddTypeExtension<ProjectedLanExtensions>()
            .AddMartenFiltering()
            .AddMartenSorting();

        services.AddGraphQLServer("guildless")
            .AddQueryType<Queries>()
            .AddMutationType<Mutations>()
            .AddMartenFiltering()
            .AddMartenSorting();

        return services;
    }
}