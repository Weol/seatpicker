using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task Update(Guid seatId, string? title, Bounds? bounds, User initiator);

    public Task Create(Guid seatId, string title, Bounds bounds, User initiator);

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

        transaction.Create(seat);
    }

    public async Task Create(Guid seatId, string title, Bounds bounds, User initiator)
    {
        if (await transaction.Exists<Seat>(seatId)) throw new SeatAlreadyExistsException { SeatId = seatId };

        var seat = new Seat(seatId, title, bounds, initiator);

        transaction.Create(seat);
    }

    public async Task Remove(Guid seatId, User initiator)
    {
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        transaction.Archive(seat);
    }
}

public class SeatAlreadyExistsException: ApplicationException
{
    public required Guid SeatId { get; init; }

    protected override string ErrorMessage => $"Seat with id {SeatId} already exists";
}
