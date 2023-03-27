using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Reservation.EventHandlers;

namespace Seatpicker.Infrastructure.Adapters;

public static class DiscordRestClientExtensions
{
    public static IServiceCollection AddDiscordClient(this IServiceCollection services, Action<DiscordClientOptions> configureAction)
    {
        services.Configure(configureAction);

        return services
            .AddSingleton(CreateDiscordSocketClient)
            .AddSingleton<IDiscordClient, DiscordClient>();
    }

    private static DiscordSocketClient CreateDiscordSocketClient(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<DiscordClientOptions>>();
        var client = new DiscordSocketClient();

        client.LoginAsync(TokenType.Bot, options.Value.Token)
            .GetAwaiter()
            .GetResult();

        return client;
    }
}
