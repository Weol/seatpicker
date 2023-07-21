using System.Text;
using System.Text.Json;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

namespace Seatpicker.IntegrationTests.Tests.Management;

public static class LanGenerator
{

    public static byte[] InvalidBackround = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

    public static byte[] CreateValidBackround()
    {
        var svg = $"<svg>{Random.Shared.NextInt64().ToString()}</svg>";
        return Encoding.UTF8.GetBytes(svg);
    }

    public static Lan CreateLan(Guid? id = null, string? title = null, byte[]? background = null)
    {
        return new Lan(id ?? Guid.NewGuid(), title ?? "Test title", background ?? CreateValidBackround());
    }

    public static LanController.CreateLanRequestModel CreateLanRequestModel(Lan lan)
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