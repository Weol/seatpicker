using Seatpicker.Application.Features.Lans;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

#pragma warning disable CS1998
public record LanResponse(
    Guid Id,
    string GuildId,
    bool Active,
    string Title,
    byte[] Background,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static LanResponse FromProjectedLan(ProjectedLan lan) => new LanResponse(
        lan.Id,
        lan.GuildId,
        lan.Active,
        lan.Title,
        lan.Background,
        lan.CreatedAt,
        lan.UpdatedAt);
}