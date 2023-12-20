using JasperFx.CodeGeneration;
using Marten;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Weasel.Core;

namespace Seatpicker.Infrastructure.Adapters.Database;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions, IConfiguration> configureAction)
    {
        services.AddValidatedOptions(configureAction);

        services.AddSingleton<IAggregateRepository, AggregateRepository>()
            .AddSingleton<IDocumentRepository, DocumentRepository>()
            .AddSingleton<ITenantProvider, TenantProvider>()
            .AddSingleton<GuildRoleMappingRepository>();

        services.AddMarten(
                provider =>
                {
                    var options = new StoreOptions();
                    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                    options.Connection(databaseOptions.ConnectionString);

                    return options;
                })
            .OptimizeArtifactWorkflow()
            .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}

internal class DatabaseOptions
{
    public string ConnectionString { get; set; } = null!;
}