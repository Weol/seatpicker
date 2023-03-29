using Discord.WebSocket;
using DiscordBot.DiscordBot;

namespace DiscordBot.Handlers;

public class ConsoleLogOnMessageReceived : IDiscordEventHandler<MessageRecieved>
{


}

public interface IDiscordEventHandler<TParameter>
{
    public void Register(DiscordSocketClient discordSocketClient);
}