using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation.Ports;

public interface IFloorplanRepository
{
    Task<Floorplan> Get();
    void Save(Seat seat);
}