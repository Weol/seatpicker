using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static CreateEndpoint.Request CreateLanRequest()
    {
        return new CreateEndpoint.Request(
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static UpdateEndpoint.Request UpdateLanRequest()
    {
        return new UpdateEndpoint.Request(
            Guid.NewGuid(),
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}