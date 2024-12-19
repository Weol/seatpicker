using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

#pragma warning disable CS1998
public record LanResponse(
    string Id,
    bool Active,
    string Title,
    byte[] Background,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static LanResponse FromProjectedLan(ProjectedLan lan) => new LanResponse(
        lan.Id,
        lan.Active,
        lan.Title,
        lan.Background,
        lan.CreatedAt,
        lan.UpdatedAt);
}