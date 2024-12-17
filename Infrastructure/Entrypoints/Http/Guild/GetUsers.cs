using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class GetUsers
{
    public static IResult Get(
        [FromRoute] string guildId,
        [FromServices] IDocumentReader documentReader)
    {
        var users = documentReader.Query<UserDocument>()
            .AsEnumerable()
            .Select(document => new User(document.Id, document.Name, document.Avatar, document.Roles));

        return TypedResults.Ok(users);
    }
}