using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordRoleMapper
{
    private readonly IDocumentRepository documentRepository;

    public DiscordRoleMapper(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task Set(string guildId, DiscordRoleMapping[] mappings)
    {
        await using var transaction = documentRepository.CreateTransaction();
        transaction.Store(new GuildRoleMapping(guildId, mappings));
        transaction.Commit();
    }

    public async Task<DiscordRoleMapping[]> Get(string guildId)
    {
        var reader = documentRepository.CreateReader();

        var mapping = await reader.Get<GuildRoleMapping>(guildId) ??
                      new GuildRoleMapping(guildId, Array.Empty<DiscordRoleMapping>());

        return mapping.Mappings;
    }

    public record GuildRoleMapping(string GuildId, DiscordRoleMapping[] Mappings) : IDocument
    {
        public string Id => GuildId;
    }
}

public record DiscordRoleMapping(string DiscordRoleId, Role Role);