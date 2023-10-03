using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Frontend;

[ApiController]
[Route("frontend")]
public class GetAllSeats
{
    [HttpGet("seats")]
    [ProducesResponseType(typeof(Response), 200)]
    public async Task<IActionResult> Get(
        [FromServices] IAggregateReader aggregateReader,
        [FromServices] IUserProvider userProvider)
    {
        var tasks = aggregateReader.Query<Domain.Seat>()
            .AsEnumerable()
            .Select(async seat =>
            {
                ReservedBy? reservedBy = null;
                if (seat.ReservedBy is not null)
                {
                    var user = await userProvider.GetById(seat.ReservedBy);
                    if (user is not null)
                    {
                        reservedBy = new ReservedBy(user.Id, user.Name, user.Avatar);
                    }
                }

                return new Seat(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy);
            });

        var seats = await Task.WhenAll(tasks);

        return new OkObjectResult(new Response(seats));
    }

    public record Response(Seat[] Seats);

    public record Seat(Guid Id, string Title, Bounds Bounds, ReservedBy? ReservedBy);

    public record ReservedBy(string Id, string Name, string? Avatar);
}