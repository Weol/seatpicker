using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Authentication;

public record TokenResponse(string Token,
    string? GuildId,
    long ExpiresAt,
    string RefreshToken,
    string UserId,
    string Nick,
    string? Avatar,
    ICollection<Role> Roles);