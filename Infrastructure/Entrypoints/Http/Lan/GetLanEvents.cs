using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("lan/{id:guid}")]
[Area("lan")]
public class GetLanEvents
{
    [HttpGet("events")]
    public async Task<ActionResult<Response[]>> GetAll(
        [FromServices] IDocumentStore documentStore)
    {
        using var documentReader = documentStore.QuerySession();

        return new OkObjectResult(lans);
    }

    public record Response(int Index, string Message);
}