using Microsoft.Extensions.Options;

namespace Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

internal static class DiscordHttpClientExtensions
{
    internal static IServiceCollection AddDiscordClient(this IServiceCollection services,
        Action<DiscordClientOptions, IConfiguration> configureAction)
    {
        services.AddOptions<DiscordClientOptions>()
            .Configure(configureAction)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<DiscordClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<DiscordClientOptions>>();
            var baseUrl = options.Value.Uri;
            var version = options.Value.Version;

            var userAgent = $"DiscordBot ({baseUrl}, {version})";

            client.BaseAddress = options.Value.Uri;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);

        });

        return services;
    }
}