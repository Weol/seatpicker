using Discord.WebSocket;
using DiscordBot.Handlers;

namespace DiscordBot.DiscordBot;

public static class DiscordBotExtensions
{
    public static WebApplicationBuilder AddDiscordBot(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(CreateDiscordSocketClient).AddHostedService<DiscordBot>();

        return builder;
    }

    public static DiscordSocketClient CreateDiscordSocketClient(IServiceProvider provider)
    {
        var client = new DiscordSocketClient();

        client.MessageReceived += HandleMessageRecieved(provider);

        return client;
    }

    private static Func<SocketMessage, Task> HandleMessageRecieved(IServiceProvider provider)
    {
        provider.GetHandlers<MessageReceivedEvent>()
    }

    private Func<Task<T, Task>> ForAll()

    private static IEnumerable<IDiscordEventHandler<T>> GetHandlers<T>(this IServiceProvider provider)
        where T : IDiscordEvent
    {
        return provider.GetRequiredService<IEnumerable<IDiscordEventHandler<T>>>();
    }
}