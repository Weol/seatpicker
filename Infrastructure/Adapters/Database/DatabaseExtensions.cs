using System.ComponentModel.DataAnnotations;
using Marten;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;

namespace Seatpicker.Infrastructure.Adapters.Database;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions, IConfiguration> configureAction)
    {
        services.AddValidatedOptions(configureAction);

        services.AddSingleton<IAggregateRepository, AggregateRepository>()
            .AddSingleton<IDocumentRepository, DocumentRepository>();

        services.AddMarten(
            provider =>
            {
                var options = new StoreOptions();
                var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.Connection(databaseOptions.ConnectionString);

                return options;
            }).OptimizeArtifactWorkflow();

        return services;
    }
}

internal class DatabaseOptions
{
    [Required] public string Host { get; set; } = null!;
    [Required] public string User { get; set; } = null!;
    [Required] public string Name { get; set; } = null!;
    [Required] public string Port { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;

    public string ConnectionString =>
        $"Server={Host};Database={Name};Port={Port};User Id={User};Password={Password};Ssl Mode=Require;Trust Server Certificate=true;";
}