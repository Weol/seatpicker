using Discord.WebSocket;
using DiscordBot.DiscordBot;

namespace DiscordBot.Handlers;

public class ConsoleLogOnMessageReceived : IDiscordEventHandler<MessageReceivedEvent>
{
    public Task Handle(MessageReceivedEvent discordEvent)
    {
        Console.WriteLine(discordEvent.Message.Content);

        return Task.CompletedTask;
    }
}