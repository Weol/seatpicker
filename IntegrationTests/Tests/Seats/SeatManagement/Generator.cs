using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.SeatManagement;

public static class Generator
{
    public static SeatController.CreateSeatRequestModel (Seat seat)
    {
        return new LanController.CreateLanRequestModel(Id: lan.Id, Title: lan.Title, Background: lan.Background);
    }

    public static LanController.UpdateLanRequestModel UpdateLanRequestModel(
        Guid id,
        string? title = null,
        byte[]? background = null)
    {
        return new LanController.UpdateLanRequestModel(Id: id, Title: title, Background: background);
    }
}