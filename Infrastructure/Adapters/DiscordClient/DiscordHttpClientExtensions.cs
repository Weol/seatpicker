using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal static class DiscordHttpClientExtensions
{
    internal static IServiceCollection AddDiscordClient(this IServiceCollection services,
        Action<DiscordClientOptions> configureAction)
    {
        services.Configure(configureAction);

        //https://discord.com/api/v{version_number}
        services.AddHttpClient<DiscordClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<DiscordClientOptions>>();
            var baseUrl = options.Value.Uri;
            var version = options.Value.Version;

            var userAgent = $"DiscordBot ({baseUrl}, {version})";

            client.BaseAddress = options.Value.Uri;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        return services
            .AddPortMapping<IDiscordLookupUser, DiscordClient>()
            .AddPortMapping<IDiscordAccessTokenProvider, DiscordClient>();
    }
}