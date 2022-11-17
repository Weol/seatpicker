namespace Application.Discord;

public record DiscordAccessToken(
    string AccessToken, 
    int ExpiresIn,
    string RefreshToken,
    IEnumerable<string> Scopes,
    string TokenType);
