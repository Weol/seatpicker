using Microsoft.Extensions.Options;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Shared;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordRoleMapper
{
    private readonly IDocumentRepository documentRepository;
    private readonly DiscordAuthenticationOptions options;

    public DiscordRoleMapper(
        IOptions<DiscordAuthenticationOptions> options,
        IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
        this.options = options.Value;
    }

    public async Task Set(DiscordRoleMapping[] mappings)
    {
        await using var transaction = documentRepository.CreateTransaction();
        transaction.Store(new GuildRoleMapping(options.GuildId, mappings));
        transaction.Commit();
    }

    public async Task<DiscordRoleMapping[]> Get(string guildId)
    {
        var reader = documentRepository.CreateReader();

        var mapping = await reader.Get<GuildRoleMapping>(options.GuildId) ??
                      new GuildRoleMapping(options.GuildId, Array.Empty<DiscordRoleMapping>());

        return mapping.Mappings;
    }

    public record GuildRoleMapping(string GuildId, DiscordRoleMapping[] Mappings) : IDocument
    {
        public string Id => GuildId;
    }
}

public record DiscordRoleMapping(
    string DiscordRoleId,
    Role Role);
