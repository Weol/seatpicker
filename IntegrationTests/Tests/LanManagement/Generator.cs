using System.Text;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

public static class Generator
{
    public static byte[] InvalidBackround = { 1, 2, 3, 4, 5, 6, 7, 8 };

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