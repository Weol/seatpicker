using System.Reflection;
using JasperFx.CodeGeneration;
using Marten;
using Marten.Storage;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Shared;
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
                var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                var options = new StoreOptions();
                return ConfigureMarten(options, databaseOptions.ConnectionString);
            })
            .InitializeWith();

        return services;
    }

    internal static StoreOptions ConfigureMarten(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.Policies.AllDocumentsAreMultiTenanted();
        options.Events.TenancyStyle = TenancyStyle.Conjoined;
        options.Advanced.DefaultTenantUsageEnabled = false;

        RegisterAllDocuments(options);
        RegisterAllEvents(options);
        
        options.AutoCreateSchemaObjects = AutoCreate.None;
        options.GeneratedCodeMode = TypeLoadMode.Dynamic;
        options.SourceCodeWritingEnabled = false;

        return options;
    }

    private static void RegisterAllDocuments(StoreOptions options)
    {
        var type = typeof(IDocument);
        var assembly = typeof(DatabaseExtensions).Assembly;
        var documents = assembly.GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Append(assembly)
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

        foreach (var document in documents)
        {
            options.RegisterDocumentType(document);
        }
    }

    private static void RegisterAllEvents(StoreOptions options)
    {
        var type = typeof(IEvent);
        var assembly = typeof(DatabaseExtensions).Assembly;
        var events = assembly.GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Append(assembly)
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

        foreach (var evt in events)
        {
            options.Events.AddEventType(evt);
        }
    }
}

internal class DatabaseOptions
{
    public string ConnectionString { get; set; } = null!;
    public string? SchemaName { get; set; }
}