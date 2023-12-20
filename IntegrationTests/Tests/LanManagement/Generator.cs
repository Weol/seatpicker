using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static CreateEndpoint.Request CreateLanRequest(string guildId)
    {
        return new CreateEndpoint.Request(
            guildId,
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static UpdateEndpoint.Request UpdateLanRequest()
    {
        return new UpdateEndpoint.Request(
            Guid.NewGuid(),
            false,
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}