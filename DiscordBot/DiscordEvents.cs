using Discord.WebSocket;

namespace DiscordBot;

public interface IDiscordEvent
{

}

public record MessageReceivedEvent(SocketMessage Message) : IDiscordEvent;