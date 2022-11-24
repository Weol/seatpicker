namespace Seatpicker.Domain.Application.UserToken;

public record DiscordAccessToken(
    string AccessToken, 
    int ExpiresIn,
    string RefreshToken,
    IEnumerable<string> Scopes,
    string TokenType);

public record DiscordUser(
    string Id, 
    string Username,
    string Avatar,
    string Discriminator);