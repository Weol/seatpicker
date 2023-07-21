using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

public partial class LanController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var lan = await lanManagementService.Get(id);

        return new OkObjectResult(new LanResponseModel(lan.Id, lan.Title, Convert.ToBase64String(lan.Background)));
    }

    public record LanResponseModel(Guid Id, string Title, string Background);
}