using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static CreateLan.Request CreateLanRequest(string guildId)
    {
        return new CreateLan.Request(
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static UpdateLan.Request UpdateLanRequest()
    {
        return new UpdateLan.Request(
            Guid.NewGuid(),
            false,
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}