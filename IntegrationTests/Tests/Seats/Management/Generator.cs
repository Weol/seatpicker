using Bogus;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

public static class Generator
{
    public static Create.Request CreateSeatRequest()
    {
        return new Create.Request(
            Title: new Faker().Hacker.Verb(),
            new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, 1));
    }

    public static Update.Request UpdateSeatRequest()
    {
        return new Update.Request(
            Guid.NewGuid(),
            Title: new Faker().Hacker.Verb(),
            new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, 1));
    }
}