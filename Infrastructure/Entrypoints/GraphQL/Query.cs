using HotChocolate.Data;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public class Query
{
    public IExecutable<Guild> GetGuild(string? hostname, [FromServices] IQuerySession querySession)
    {
        IQueryable<Guild> query = querySession.Query<Guild>();
        if (hostname is not null)
        {
            query = query.Where(guild => guild.Hostnames.Contains(hostname));
        }
        return query.AsExecutable();
    }

    public async Task<IEnumerable<UndiscoveredGuild>> GetUndiscoveredGuilds([FromServices] DiscordAdapter discordAdapter)
    {
       return (await discordAdapter.GetGuilds())
           .Select(guild => new UndiscoveredGuild(guild.Id, guild.Name, guild.Icon));
    }
}

public record UndiscoveredGuild(string Id, string Name, string? Icon);

[ExtendObjectType(typeof(Guild))]
public class GuildExtensions
{
    public IExecutable<ProjectedLan> GetLan([FromServices] IQuerySession querySession, [Parent] Guild guild, string? id)
    {
        return query.AsExecutable();
    }
}

[ExtendObjectType(typeof(ProjectedLan))]
public class ProjectedLanExtensions
{
    public IExecutable<ProjectedSeat> GetSeats(
        [FromServices] IQuerySession querySession,
        [Parent] ProjectedLan lan,
        string? id)
    {
        var query = querySession.Query<ProjectedSeat>().Where(seat => seat.LanId == lan.Id);
        if (id is not null) query = query.Where(seat => seat.Id == id);
        return query.AsExecutable();
    }
}