using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("lan")]
[Area("lan")]
public class GetEndpoint
{
    [HttpGet("")]
    public Task<ActionResult<Response[]>> GetAll(
        [FromServices] IDocumentReader documentReader)
    {
        var lans = documentReader.Query<ProjectedLan>()
            .OrderByDescending(lan => lan.CreatedAt)
            .AsEnumerable()
            .Select(lan => new Response(lan.Id, lan.GuildId, lan.Active, lan.Title, lan.Background, lan.CreatedAt, lan.UpdatedAt));

        return Task.FromResult<ActionResult<Response[]>>(new OkObjectResult(lans));
    }

    [HttpGet("{id:guid}")]
    public Task<ActionResult<Response>> Get(
        [FromRoute] Guid id,
        [FromServices] IDocumentReader documentReader)
    {
        var lan = documentReader.Query<ProjectedLan>()
            .SingleOrDefault(lan => lan.Id == id);

        if (lan is null) return Task.FromResult<ActionResult<Response>>(new NotFoundResult());

        return Task.FromResult<ActionResult<Response>>(
            new OkObjectResult(new Response(lan.Id, lan.GuildId, lan.Active, lan.Title, lan.Background, lan.CreatedAt, lan.UpdatedAt)));
    }

    [HttpGet("active")]
    public Task<ActionResult<Response>> GetActiveLan(
        [FromQuery] string guildId,
        [FromServices] IDocumentReader documentReader)
    {
        var lan = documentReader.Query<ProjectedLan>()
            .Where(lan => lan.GuildId == guildId)
            .SingleOrDefault(lan => lan.Active);

        if (lan is null) return Task.FromResult<ActionResult<Response>>(new NotFoundResult());

        return Task.FromResult<ActionResult<Response>>(
            new OkObjectResult(new Response(lan.Id, lan.GuildId, lan.Active, lan.Title, lan.Background, lan.CreatedAt, lan.UpdatedAt)));
    }
    
    public record Response(Guid Id, string GuildId, bool Active, string Title, byte[] Background,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}