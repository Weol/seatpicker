using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static LanController.CreateLanRequest CreateLanRequest()
    {
        return new LanController.CreateLanRequest(
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static LanController.UpdateLanRequest UpdateLanRequest()
    {
        return new LanController.UpdateLanRequest(
            Guid.NewGuid(),
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}