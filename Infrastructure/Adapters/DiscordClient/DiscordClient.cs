using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Reservation.EventHandlers;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

public class DiscordClient : IDiscordClient
{
    private readonly DiscordRestClient discordRestClient;
    private readonly string guildId;

    public DiscordRestClientProvider(DiscordRestClient discordRestClient, IOptions<DiscordClientOptions>)
    {
        this.discordRestClient = discordRestClient;
        guildId = options.Value.GuildId;
    }

    private DiscordRestClient Get(string token)
    {
    }

    public void Dispose()
    {
        discordRestClient.Dispose();
    }

    public ValueTask DisposeAsync() => discordRestClient.DisposeAsync();

    public Task StartAsync() => ((IDiscordClient) discordRestClient).StartAsync();

    public Task StopAsync() => ((IDiscordClient) discordRestClient).StopAsync();

    public Task<IApplication> GetApplicationInfoAsync(RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetApplicationInfoAsync(options);

    public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetChannelAsync(id, mode, options);

    public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetPrivateChannelsAsync(mode, options);

    public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetDMChannelsAsync(mode, options);

    public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetGroupChannelsAsync(mode, options);

    public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetConnectionsAsync(options);

    public Task<IApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetGlobalApplicationCommandAsync(id, options);

    public Task<IReadOnlyCollection<IApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string? locale = null, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetGlobalApplicationCommandsAsync(withLocalizations, locale, options);

    public Task<IApplicationCommand> CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).CreateGlobalApplicationCommand(properties, options);

    public Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).BulkOverwriteGlobalApplicationCommand(properties, options);

    public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetGuildAsync(id, mode, options);

    public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetGuildsAsync(mode, options);

    public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream? jpegIcon = null, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).CreateGuildAsync(name, region, jpegIcon, options);

    public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetInviteAsync(inviteId, options);

    public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetUserAsync(id, mode, options);

    public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetUserAsync(username, discriminator, options);

    public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetVoiceRegionsAsync(options);

    public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetVoiceRegionAsync(id, options);

    public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions? options = null) => ((IDiscordClient) discordRestClient).GetWebhookAsync(id, options);

    public Task<int> GetRecommendedShardCountAsync(RequestOptions? options = null) => discordRestClient.GetRecommendedShardCountAsync(options);

    public Task<BotGateway> GetBotGatewayAsync(RequestOptions? options = null) => discordRestClient.GetBotGatewayAsync(options);

    public ConnectionState ConnectionState => ((IDiscordClient) discordRestClient).ConnectionState;

    public ISelfUser CurrentUser => ((IDiscordClient) discordRestClient).CurrentUser;

    public TokenType TokenType => discordRestClient.TokenType;
}
