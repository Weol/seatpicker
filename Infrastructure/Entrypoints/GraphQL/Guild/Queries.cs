using HotChocolate.Data;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL.Guild;

public class Queries
{
    public IExecutable<Application.Features.Lan.Guild> GetGuild(string? hostname, [FromServices] IQuerySession querySession)
    {
        var query = querySession.Query<Application.Features.Lan.Guild>().AsQueryable();
        if (hostname is not null) query = query.Where(guild => guild.Hostnames.Contains(hostname));
        return query.AsExecutable();
    }
}

[ExtendObjectType(typeof(Application.Features.Lan.Guild))]
public class GuildExtensions
{
    public IExecutable<ProjectedLan> GetLan([FromServices] IQuerySession querySession, [Parent] Application.Features.Lan.Guild guild, string? id)
    {
        var query = querySession.Query<ProjectedLan>().AsQueryable();
        if (id is not null) query = query.Where(lan => lan.Id == id);
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