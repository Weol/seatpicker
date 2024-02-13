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
                var options = new StoreOptions();
                var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                if (databaseOptions.SchemaName is not null) options.DatabaseSchemaName = databaseOptions.SchemaName;

                options.Connection(databaseOptions.ConnectionString);
                options.Policies.AllDocumentsAreMultiTenanted();
                options.Events.TenancyStyle = TenancyStyle.Conjoined;
                options.Advanced.DefaultTenantUsageEnabled = false;
                
                options.AutoCreateSchemaObjects = AutoCreate.None;
                options.GeneratedCodeMode = TypeLoadMode.Dynamic;

                RegisterAllDocuments(options, provider.GetRequiredService<ILogger<IDocument>>());

                return options;
            })
            .ApplyAllDatabaseChangesOnStartup()
            .InitializeWith();

        return services;
    }

    internal static void RegisterAllDocuments(StoreOptions options, ILogger<IDocument> logger)
    {
        var type = typeof(IDocument);
        var documents = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName.StartsWith("Seatpicker.Application"));
        
        foreach (var document in documents)
        {
            logger.LogInformation("Registering document type {DocumentType}", document.FullName);
            options.RegisterDocumentType(document);
        }
    }
}

internal class DatabaseOptions
{
    public string ConnectionString { get; set; } = null!;
    public string? SchemaName { get; set; }
}