using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static Create.Request CreateLanRequest()
    {
        return new Create.Request(
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static Update.Request UpdateLanRequest()
    {
        return new Update.Request(
            Guid.NewGuid(),
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}