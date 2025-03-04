﻿using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Adapters.Discord;

internal static class DiscordAdapterExtensions
{
    internal static IServiceCollection AddDiscordAdapter(this IServiceCollection services,
        Action<DiscordAdapterOptions, IConfiguration> configureAction)
    {
        services.AddOptions<DiscordAdapterOptions>()
            .Configure(configureAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddMemoryCache()
            .AddHttpClient<DiscordAdapter>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<DiscordAdapterOptions>>();
                var baseUrl = options.Value.Uri;
                var version = options.Value.Version;

                var userAgent = $"DiscordBot ({baseUrl}, {version})";

                client.BaseAddress = options.Value.Uri;
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });

        services.AddTransient<IDiscordGuildProvider>(provider => provider.GetRequiredService<DiscordAdapter>());

        return services;
    }
}