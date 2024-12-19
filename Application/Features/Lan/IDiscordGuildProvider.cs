namespace Seatpicker.Application.Features.Lan;

public interface IDiscordGuildProvider
{
    public IAsyncEnumerable<DiscordGuild> GetAll();
    
    public async Task<DiscordGuild?> Find(string id) {
        await foreach (var discordGuild in GetAll())
        {
            if (discordGuild.Id == id) return discordGuild;
        }

        return null;
    }
}

public record DiscordGuild(string Id, string Name, string? Icon, DiscordGuildRole[] Roles);

public record DiscordGuildRole(string Id, string Name, int Color, string? Icon);
