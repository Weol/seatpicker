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
    public async Task<ActionResult<Response[]>> GetAll(
        [FromServices] IDocumentReader documentReader)
    {
        var lans = documentReader.Query<ProjectedLan>()
            .AsEnumerable()
            .Select(lan => new Response(lan.Id, lan.Title, lan.Background));

        return new OkObjectResult(lans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Response>> Get(
        [FromRoute] Guid id,
        [FromServices] IDocumentReader documentReader)
    {
        var lan = documentReader.Query<ProjectedLan>()
            .SingleOrDefault(lan => lan.Id == id);

        if (lan is null) return new NotFoundResult();

        return new OkObjectResult(new Response(lan.Id, lan.Title, lan.Background));
    }

    public record Response(Guid Id, string Title, byte[] Background);
}