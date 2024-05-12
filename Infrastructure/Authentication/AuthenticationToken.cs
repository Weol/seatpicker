using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Authentication;

public record AuthenticationToken(
    string Id,
    string Nick,
    string? Avatar,
    string RefreshToken,
    Role[] Roles,
    string? GuildId);