using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public partial class LanController
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var lan = await lanQueries.GetLan(id);

        if (lan is null) return new NotFoundResult();

        return new OkObjectResult(new LanResponseModel(lan.Id, lan.Title, Convert.ToBase64String(lan.Background)));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lan = (await lanQueries.GetAllLan())
            .Select(lan => new LanResponseModel(lan.Id, lan.Title, Convert.ToBase64String(lan.Background)));

        return new OkObjectResult(lan);
    }

    public record LanResponseModel(Guid Id, string Title, string Background);
}