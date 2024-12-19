using System.ComponentModel.DataAnnotations;

namespace Seatpicker.Infrastructure.Adapters.Discord;

public class DiscordAdapterOptions
{
    [Required]
    public string ClientId { get; set; } = null!;

    [Required]
    public string ClientSecret { get; set; } = null!;

    [Required]
    public string BotToken { get; set; } = null!;

    [Required]
    public Uri Uri { get; set; } = null!;

    [Required]
    public int Version => GetVersionFromDiscordUri(Uri);

    private static int GetVersionFromDiscordUri(Uri baseUri)
    {
        var version = baseUri.Segments.Single(x => x.StartsWith('v'));
        if (int.TryParse(version.Trim('v').Trim('/'), out var number))
        {
            return number;
        }

        throw new UriFormatException("Invalid discord uri");
    }
}