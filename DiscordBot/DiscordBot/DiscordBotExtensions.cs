using Discord.WebSocket;
using DiscordBot.Handlers;

namespace DiscordBot.DiscordBot;

public static class DiscordBotExtensions
{
    public static WebApplicationBuilder AddDiscordBot(this WebApplicationBuilder builder)
    {
        var discordSocketClient = new DiscordSocketClient();
        builder.Services
            .AddSingleton(discordSocketClient)
            .RegisterDiscordEventHandlers(discordSocketClient)
            .AddHostedService<DiscordBot>();

        return builder;
    }

    private static IServiceCollection RegisterDiscordEventHandlers(this IServiceCollection services, DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.MessageReceived +=
        foreach (var handler in GetAllDiscordEventHandlers())
        {
            handler.
        }
    }

    public static Type[] GetAllDiscordEventHandlers()
    {
        return typeof(Program)
            .Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDiscordEventHandler)))
            .ToArray();
    }
}
