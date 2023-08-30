using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

public partial class LanController
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var lan = await lanManagementService.Get(id);

        return new OkObjectResult(new LanResponseModel(lan.Id, lan.Title, Convert.ToBase64String(lan.Background)));
    }

    public record LanResponseModel(Guid Id, string Title, string Background);
}