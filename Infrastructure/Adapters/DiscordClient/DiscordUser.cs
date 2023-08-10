namespace Seatpicker.Infrastructure.Entrypoints.Http;

public record DiscordUser(
    string Id,
    string Username,
    string? Avatar);