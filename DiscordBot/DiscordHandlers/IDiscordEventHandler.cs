namespace DiscordBot.Handlers;

public interface IDiscordEventHandler<T>
    where T : IDiscordEvent
{
    Task Handle(T discordEvent);
}