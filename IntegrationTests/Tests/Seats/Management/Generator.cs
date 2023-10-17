using Bogus;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

public static class Generator
{
    public static CreateEndpoint.Request CreateSeatRequest(Guid? lanId = null)
    {
        return new CreateEndpoint.Request(
            Title: new Faker().Hacker.Verb(),
            LanId: lanId ?? new Guid(),
            Bounds:new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, 1));
    }

    public static UpdateEndpoint.Request UpdateSeatRequest()
    {
        return new UpdateEndpoint.Request(
            Guid.NewGuid(),
            Title: new Faker().Hacker.Verb(),
            new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, 1));
    }
}