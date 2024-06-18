using HotChocolate.Data;

// ReSharper disable InconsistentNaming
namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQLEntrypoint(this IServiceCollection services)
    {
        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddTypeExtension<GuildExtensions>()
            .AddTypeExtension<ProjectedLanExtensions>()
            .AddMartenFiltering()
            .AddMartenSorting();

        return services;
    }
}