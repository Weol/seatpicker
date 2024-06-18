using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Guilds;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public class Mutation
{
    public async Task<Guild> UpsertGuild(Guild guild, [FromServices] GuildService guildService)
    {
        return await guildService.Update(guild);
    }
}
