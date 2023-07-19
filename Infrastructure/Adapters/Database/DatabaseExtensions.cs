using System.ComponentModel.DataAnnotations;
using Marten;
using Microsoft.Extensions.Options;
using Weasel.Core;

namespace Seatpicker.Infrastructure.Adapters.Database;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, Action<DatabaseOptions> configureAction)
    {
        services.AddValidatedOptions(configureAction);

        services.AddMarten(provider =>
        {
            var options = new StoreOptions();
            var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.Connection(databaseOptions.ConnectionString);

            var environment = provider.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }

            return options;
        });

        return services;
    }
}

internal class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = null!;
}