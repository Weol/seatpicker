namespace Seatpicker.SeatContext;

public record DiscordUser(
    string Id, 
    string Username,
    string Avatar,
    string Discriminator);