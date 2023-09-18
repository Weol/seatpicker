﻿using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Adapters;

public static class AdapterExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services.AddDatabase(ConfigureDatabase).AddUserProvider();
    }

    private static void ConfigureDatabase(DatabaseOptions options, IConfiguration configuration)
    {
        configuration.GetSection("Database").Bind(options);
    }

}