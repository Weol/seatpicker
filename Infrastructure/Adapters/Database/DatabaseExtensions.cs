using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JasperFx.CodeGeneration;
using Marten;
using Marten.Events;
using Marten.Storage;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lan;
using Shared;
using Weasel.Core;

namespace Seatpicker.Infrastructure.Adapters.Database;

internal static class DatabaseExtensions
{
    public static int SessionIdCounter = 1;
    
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions, IConfiguration> configureAction)
    {
        services.AddValidatedOptions(configureAction);

        services.AddSingleton<AggregateRepository>()
            .AddSingleton<DocumentRepository>()
            .AddScoped(CreateDocumentTransaction)
            .AddScoped(CreateDocumentReader)
            .AddScoped(CreateAggregateTransaction)
            .AddScoped(CreateGuildlessDocumentTransaction)
            .AddScoped(CreateGuildlessDocumentReader)
            .AddScoped(CreateGuildlessAggregateTransaction);

        services.AddMarten(
                provider =>
                {
                    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                    var options = new StoreOptions();
                    return ConfigureMarten(options, databaseOptions.ConnectionString);
                })
            .InitializeWith()
            .OptimizeArtifactWorkflow();
        
        return services;
    }
    
    private static IDocumentTransaction CreateDocumentTransaction(IServiceProvider provider)
    {
        var guildIdProvider = provider.GetRequiredService<GuildIdProvider>();

        var repository = provider.GetRequiredService<DocumentRepository>();
        return repository.CreateTransaction(guildIdProvider.GuildId);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
    private static IAggregateTransaction CreateAggregateTransaction(IServiceProvider provider)
    {
        var guildIdProvider = provider.GetRequiredService<GuildIdProvider>();
        
        var repository = provider.GetRequiredService<AggregateRepository>();
        return repository.CreateTransaction(guildIdProvider.GuildId);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
    private static IDocumentReader CreateDocumentReader(IServiceProvider provider)
    {
        var guildIdProvider = provider.GetRequiredService<GuildIdProvider>();

        var repository = provider.GetRequiredService<DocumentRepository>();
        return repository.CreateReader(guildIdProvider.GuildId);
    }

    private static IGuildlessDocumentReader CreateGuildlessDocumentReader(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<DocumentRepository>();
        return repository.CreateGuildlessReader();
    }

    private static IGuildlessDocumentTransaction CreateGuildlessDocumentTransaction(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<DocumentRepository>();
        return repository.CreateGuildlessTransaction();
    }

    private static IGuildlessAggregateTransaction CreateGuildlessAggregateTransaction(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<AggregateRepository>();
        return repository.CreateGuildlessTransaction();
    }

    internal static StoreOptions ConfigureMarten(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.Policies.AllDocumentsAreMultiTenanted();
        options.Events.TenancyStyle = TenancyStyle.Conjoined;
        options.Events.StreamIdentity = StreamIdentity.AsString;

        RegisterAllDocuments(options);
        RegisterAllEvents(options);

        options.AutoCreateSchemaObjects = AutoCreate.All;
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

        options.Schema.For<Guild>().SingleTenanted();
    }

    private static void RegisterAllEvents(StoreOptions options)
    {
        var type = typeof(Shared.IEvent);
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