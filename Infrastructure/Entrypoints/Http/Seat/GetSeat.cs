using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class GetSeat
{
    public static async Task<IResult> GetAll(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromServices] DocumentRepository documentRepository,
        [FromServices] IUserProvider userProvider)
    {
        using var documentReader = documentRepository.CreateReader(guildId);

        var tasks = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .AsEnumerable()
            .Select(
                async seat =>
                {
                    User? reservedBy = null;
                    if (seat.ReservedBy is not null)
                    {
                        var user = await userProvider.GetById(seat.ReservedBy);
                        if (user is not null)
                        {
                            reservedBy = new User(user.Id, user.Name, user.Avatar);
                        }
                    }

                    return new SeatResponse(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy);
                });

        var seats = await Task.WhenAll(tasks);

        return TypedResults.Ok(seats);
    }

    public static async Task<IResult> Get(
        [FromRoute] string guildId,
        [FromRoute] string lanId,
        [FromRoute] string seatId,
        [FromServices] IUserProvider userProvider,
        [FromServices] DocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader(guildId);

        var seat = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .SingleOrDefault(seat => seat.Id == seatId);

        if (seat is null) return TypedResults.NotFound();

        User? reservedBy = null;
        if (seat.ReservedBy is not null)
        {
            var user = await userProvider.GetById(seat.ReservedBy);
            if (user is not null)
            {
                reservedBy = new User(user.Id, user.Name, user.Avatar);
            }
        }

        return TypedResults.Ok(new SeatResponse(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy));
    }
}