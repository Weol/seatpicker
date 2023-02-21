using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters.Database.Repositories;

public class FloorplanRepository : IFloorplanRepository
{


    public Task<Floorplan> Get() => throw new NotImplementedException();

    public void Save(Seat seat)
    {
        throw new NotImplementedException();
    }
}