using HotChocolate.Data;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public class Queries
{
    public async Task<IEnumerable<UndiscoveredGuild>> GetUndiscoveredGuilds(
        [FromServices] DiscordAdapter discordAdapter)
    {
        return (await discordAdapter.GetGuilds()).Select(
            guild => new UndiscoveredGuild(guild.Id, guild.Name, guild.Icon));
    }
}

public record UndiscoveredGuild(string Id, string Name, string? Icon);