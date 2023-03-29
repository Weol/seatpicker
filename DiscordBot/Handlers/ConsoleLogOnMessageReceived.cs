using Discord.WebSocket;
using DiscordBot.DiscordBot;

namespace DiscordBot.Handlers;

public class ConsoleLogOnMessageReceived : IDiscordEventHandler<SocketMessage>
{
    public void Register(DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.MessageReceived +=
    }


}

public interface IDiscordEventHandler<TParameter>
{
    public void Register(DiscordSocketClient discordSocketClient);
}