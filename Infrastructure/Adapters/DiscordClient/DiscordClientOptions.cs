namespace Seatpicker.Infrastructure.Adapters;

public class DiscordClientOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public Uri RedirectUri { get; set; } = null!;
    public Uri Uri { get; set; } = null!;
    public int Version => GetVersionFromDiscordUri(Uri);

    private static int GetVersionFromDiscordUri(Uri baseUri)
    {
        var version = baseUri.Segments.Single(x => x.StartsWith("v"));
        if (int.TryParse(version.Trim('v').Trim('/'), out var number))
        {
            return number;
        }

        throw new UriFormatException("Invalid discord uri");
    }
}