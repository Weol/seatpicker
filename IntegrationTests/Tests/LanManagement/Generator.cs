using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static LanController.CreateLanRequestModel CreateLanRequestModel()
    {
        return new LanController.CreateLanRequestModel(
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }

    public static LanController.UpdateLanRequestModel UpdateLanRequestModel()
    {
        return new LanController.UpdateLanRequestModel(
            Guid.NewGuid(),
            new Faker().Hacker.Noun(),
            LanGenerator.CreateValidBackround());
    }
}