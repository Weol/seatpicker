namespace Seatpicker.Infrastructure.Adapters.DiscordClient;

public record DiscordUser(
    string Id,
    string Username,
    string? Avatar);