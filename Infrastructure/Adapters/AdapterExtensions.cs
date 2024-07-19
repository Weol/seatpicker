using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services
            .AddDatabase(ConfigureDatabase)
            .AddDiscordAdapter(ConfigureDiscordAdapter);
    }

    private static void ConfigureDatabase(DatabaseOptions options, IConfiguration configuration)
    {
        var section = configuration.GetSection("Database");

        var connectionString = section.GetValue<string>("ConnectionString");
        if (connectionString != null)
        {
            options.ConnectionString = connectionString;
            return;
        }

        var environmentVariable = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING", EnvironmentVariableTarget.User);
        if (environmentVariable != null)
        {
            options.ConnectionString = environmentVariable;
            return;
        }

        var host = section.GetValue<string>("Host");
        var name = section.GetValue<string>("Name");
        var port = section.GetValue<string>("Port");

        // Values from keyvault
        var password = configuration["DatabaseAdminPassword"] ?? throw new ArgumentException("Database admin password cannot be null");
        var user = configuration["DatabaseAdminUsername"] ?? throw new ArgumentException("Database admin username cannot be null");

        options.ConnectionString =
            $"Server={host};Database={name};Port={port};User Id={user};Password={password};Ssl Mode=Require;Trust Server Certificate=true;";
    }

    private static void ConfigureDiscordAdapter(DiscordAdapterOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Discord").Bind(options);

        // Configuration values from key vault
        options.ClientId = configuration["DiscordClientId"] ?? throw new ArgumentException("Discord client id cannot be null");
        options.ClientSecret = configuration["DiscordClientSecret"] ?? throw new ArgumentException("Discord client secret cannot be null");
        options.BotToken = configuration["DiscordBotToken"] ?? throw new ArgumentException("Discord bot token cannot be null");
    }
}