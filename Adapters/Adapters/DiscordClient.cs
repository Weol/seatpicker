using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seatpicker.Domain.Application.UserToken;
using Seatpicker.Domain.Domain.Registration.Ports;

namespace Seatpicker.Adapters.Adapters;

internal class DiscordClient : IDiscordAccessTokenProvider, IDiscordLookupUser
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly DiscordClientOptions options;
    private readonly ILogger<DiscordClient> logger;

    public DiscordClient(HttpClient httpClient, IOptions<DiscordClientOptions> options,
        JsonSerializerOptions jsonSerializerOptions, ILogger<DiscordClient> logger)
    {
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<DiscordAccessToken> GetFor(string discordToken)
    {
        var response = await httpClient.PostAsync("oauth2/token", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", options.ClientId),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", options.RedirectUri.ToString()),
            new KeyValuePair<string, string>("code", discordToken),
        }));

        return await DeserializeContent<DiscordAccessToken>(response);
    }

    public async Task<DiscordUser> Lookup(string accessToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "users/@me");

        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(requestMessage);
        return await DeserializeContent<DiscordUser>(response);
    }

    private async Task<TModel> DeserializeContent<TModel>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Discord API responded with code {StatusCode} and body {body}", response.StatusCode, body);
            
            return JsonSerializer.Deserialize<TModel>(body, jsonSerializerOptions)
                   ?? throw new NullReferenceException($"Could not deserialize Discord response to {nameof(TModel)}");
        }

        logger.LogError("Non-successful response code from Discord {@ResponseInfo}", new
        {
            response.StatusCode,
            Body = body,
        });

        throw new DiscordException($"Non-successful response code from Discord {response.StatusCode}");
    }
}

internal class DiscordException : Exception
{
    public Exception? Exception { get; set; }

    public DiscordException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DiscordException(string message) : base(message)
    {
    }
}

internal class DiscordClientOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public Uri RedirectUri { get; set; } = null!;
    public Uri BaseUri { get; set; } = null!;
    public int Version => GetVersionFromDiscordUri(BaseUri);

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

internal static class DiscordHttpClientExtensions
{
    internal static IServiceCollection AddDiscordClient(this IServiceCollection services,
        Action<DiscordClientOptions> configureAction)
    {
        services.Configure(configureAction);

        //https://discord.com/api/v{version_number}
        services.AddHttpClient<DiscordClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<DiscordClientOptions>>();
            var baseUrl = options.Value.BaseUri;
            var version = options.Value.Version;

            var userAgent = $"DiscordBot ({baseUrl}, {version})";

            client.BaseAddress = options.Value.BaseUri;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        return services
            .AddSingletonPortMapping<IDiscordLookupUser, DiscordClient>()
            .AddSingletonPortMapping<IDiscordAccessTokenProvider, DiscordClient>();
    }
}