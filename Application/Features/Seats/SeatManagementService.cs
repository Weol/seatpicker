using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task Update(Guid seatId, string? title, Bounds? bounds, User initiator);

    public Task<Guid> Create(string title, Bounds bounds, User initiator);

    public Task Remove(Guid seatId, User initiator);
}

public class SeatManagementService : ISeatManagementService
{
    private readonly IAggregateTransaction transaction;

    public SeatManagementService(IAggregateTransaction transaction)
    {
        this.transaction = transaction;
    }

    public async Task Update(Guid seatId, string? title, Bounds? bounds, User initiator)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        if (title is not null) seat.SetTitle(title, initiator);
        if (bounds is not null) seat.SetBounds(bounds, initiator);

        transaction.Update(seat);
    }

    public async Task<Guid> Create(string title, Bounds bounds, User initiator)
    {
        var id = Guid.NewGuid();

        var seat = new Seat(id, title, bounds, initiator);

        transaction.Create(seat);

        return id;
    }

    public async Task Remove(Guid seatId, User initiator)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.Archive(initiator);

        transaction.Archive(seat);
    }
}