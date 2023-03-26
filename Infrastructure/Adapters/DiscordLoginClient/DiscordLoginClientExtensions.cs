using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal static class DiscordLoginClientExtensions
{
    internal static IServiceCollection AddDiscordLoginClient(this IServiceCollection services,
        Action<DiscordLoginClientOptions> configureAction)
    {
        services.Configure(configureAction);

        //https://discord.com/api/v{version_number}
        services.AddHttpClient<Adapters.DiscordLoginClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<DiscordLoginClientOptions>>();
            var baseUrl = options.Value.Uri;
            var version = options.Value.Version;

            var userAgent = $"DiscordBot ({baseUrl}, {version})";

            client.BaseAddress = options.Value.Uri;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        return services
            .AddPortMapping<IDiscordLookupUser, Adapters.DiscordLoginClient>()
            .AddPortMapping<IDiscordAccessTokenProvider, Adapters.DiscordLoginClient>();
    }
}