using Seatpicker.Application.Features.Lans;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public interface ISeatManagementService
{
    public Task Update(Guid lanId, Guid seatId, string? title, Bounds? bounds, User initiator);

    public Task<Guid> Create(Guid lanId, string title, Bounds bounds, User initiator);

    public Task Remove(Guid lanId, Guid seatId, User initiator);
}

public class SeatManagementService : ISeatManagementService
{
    private readonly IAggregateRepository repository;

    public SeatManagementService(IAggregateRepository repository)
    {
        this.repository = repository;
    }

    public async Task Update(Guid lanId, Guid seatId, string? title, Bounds? bounds, User initiator)
    {
        await using var transaction = repository.CreateTransaction();
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        if (title is not null) seat.SetTitle(title, initiator);
        if (bounds is not null) seat.SetBounds(bounds, initiator);

        transaction.Update(seat);
        transaction.Commit();
    }

    public async Task<Guid> Create(Guid lanId, string title, Bounds bounds, User initiator)
    {
        await using var transaction = repository.CreateTransaction();
        var id = Guid.NewGuid();

        var lan = await transaction.Aggregate<Lan>(lanId)
            ?? throw new LanNotFoundException { LanId = lanId };

        var seat = new Seat(id, lan, title, bounds, initiator);

        transaction.Create(seat);
        transaction.Commit();

        return id;
    }

    public async Task Remove(Guid lanId, Guid seatId, User initiator)
    {
        await using var transaction = repository.CreateTransaction();
        var seat = await transaction.Aggregate<Seat>(seatId) ?? throw new SeatNotFoundException{ SeatId = seatId };

        seat.Archive(initiator);

        transaction.Update(seat);
        transaction.Archive(seat);

        transaction.Commit();
    }
}