namespace Seatpicker.Application.Features.Guilds;

public interface IDiscordGuildProvider
{
    public IAsyncEnumerable<DiscordGuild> GetAll();
}

public record DiscordGuild(string Id, string Name, string? Icon, DiscordGuildRole[] Roles);

public record DiscordGuildRole(string Id, string Name, int Color, string? Icon);
