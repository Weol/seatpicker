using System.ComponentModel.DataAnnotations;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
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
            .AddScoped(CreateAggregateTransaction)
            .AddScoped(CreateAggregateReader)
            .AddSingleton<IDocumentRepository, DocumentRepository>()
            .AddScoped(CreateDocumentTransaction)
            .AddScoped(CreateDocumentReader);

        services.AddMarten(
            provider =>
            {
                var options = new StoreOptions();
                var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.Connection(databaseOptions.ConnectionString);

                var environment = provider.GetRequiredService<IHostEnvironment>();
                if (environment.IsDevelopment())
                {
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                }

                return options;
            })
            .AddAsyncDaemon(DaemonMode.HotCold);

        return services;
    }

    private static IAggregateTransaction CreateAggregateTransaction(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<IAggregateRepository>();
        return repository.CreateTransaction();
    }

    private static IAggregateReader CreateAggregateReader(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<IAggregateRepository>();
        return repository.CreateReader();
    }

    private static IDocumentTransaction CreateDocumentTransaction(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<IDocumentRepository>();
        return repository.CreateTransaction();
    }

    private static IDocumentReader CreateDocumentReader(IServiceProvider provider)
    {
        var repository = provider.GetRequiredService<IDocumentRepository>();
        return repository.CreateReader();
    }
}

internal class DatabaseOptions
{
    [Required] public string ConnectionString { get; set; } = null!;
}