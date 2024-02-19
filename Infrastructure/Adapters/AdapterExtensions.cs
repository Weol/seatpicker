﻿using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.Discord;
using Seatpicker.Infrastructure.Adapters.SignalR;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services
            .AddDatabase(ConfigureDatabase)
            .AddSignalRAdapter()
            .AddDiscordAdapter(ConfigureDiscordAdapter)
            .AddUserManager();
    }

    public static WebApplication UseAdapters(this WebApplication app)
    {
        app.UseSignalRAdapter();

        return app;
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
        var password = configuration["DatabaseAdminPassword"] ?? throw new NullReferenceException();
        var user = configuration["DatabaseAdminUsername"] ?? throw new NullReferenceException();

        options.ConnectionString =
            $"Server={host};Database={name};Port={port};User Id={user};Password={password};Ssl Mode=Require;Trust Server Certificate=true;";
    }
    
    private static void ConfigureDiscordAdapter(DiscordAdapterOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Discord").Bind(options);

        // Configuration values from key vault
        options.ClientId = configuration["DiscordClientId"] ?? throw new NullReferenceException();
        options.ClientSecret = configuration["DiscordClientSecret"] ?? throw new NullReferenceException();
        options.BotToken = configuration["DiscordBotToken"] ?? throw new NullReferenceException();
    }
}