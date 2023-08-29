using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task UpdateBounds(Guid seatId, Bounds bounds, User initiator);

    public Task CreateSeat(Guid seatId, string title, Bounds bounds, User initiator);

    public Task DeleteSeat(Guid seatId, User initiator);
}

public class SeatManagementService : ISeatManagementService
{
    private readonly IAggregateTransaction transaction;

    public SeatManagementService(IAggregateTransaction transaction)
    {
        this.transaction = transaction;
    }

    public Task UpdateBounds(Guid seatId, Bounds bounds, User initiator) => throw new NotImplementedException();

    public async Task CreateSeat(Guid seatId, string title, Bounds bounds, User initiator)
    {
        if (await transaction.Exists<Seat>(seatId)) throw new SeatAlreadyExistsException { SeatId = seatId };

        var seat = new Seat(seatId, title, bounds, initiator);

        transaction.Create(seat);
    }

    public Task DeleteSeat(Guid seatId, User initiator) => throw new NotImplementedException();
}

public class SeatAlreadyExistsException: ApplicationException
{
    public required Guid SeatId { get; init; }

    public override string Message => $"Seat with id {SeatId} already exists";
}
