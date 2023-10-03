using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("lan")]
public class Get
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response), 200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromServices] IAggregateReader aggregateReader,
        [FromServices] IUserProvider userProvider)
    {
        var lan = aggregateReader.Query<Domain.Lan>()
            .SingleOrDefault(lan => lan.Id == id);

        if (lan is null) return new NotFoundResult();

        return new OkObjectResult(new Response(lan.Id, lan.Title, lan.Background));
    }

    public record Response(Guid Id, string Title, byte[] Background);
}