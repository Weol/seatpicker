using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;

public class GuildRoleMappingRepository
{
    private readonly IDocumentRepository documentRepository;

    public GuildRoleMappingRepository(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task SaveRoleMapping(string guildId, IEnumerable<(string RoleId, Domain.Role Role)> mappings)
    {
        using var transaction = documentRepository.CreateTransaction();

        var transformed = mappings
            .Select(mapping => new GuildRoleMappingEntry(mapping.RoleId, mapping.Role))
            .ToArray();

        transaction.Store(new GuildRoleMapping(
            guildId,
            transformed));

        await transaction.Commit();
    }

    public async IAsyncEnumerable<(string RoleId, Role Role)> GetRoleMapping(string guildId)
    {
        using var reader = documentRepository.CreateReader();

        var roleMappings = await reader.Get<GuildRoleMapping>(guildId) ??
            new GuildRoleMapping(guildId, Array.Empty<GuildRoleMappingEntry>());

        foreach (var roleMappingsMapping in roleMappings.Mappings)
        {
            yield return (roleMappingsMapping.RoleId, roleMappingsMapping.Role);
        }
    }
}

public record GuildRoleMapping(string GuildId, IEnumerable<GuildRoleMappingEntry> Mappings) : IDocument
{
    public string Id => GuildId;
}

public record GuildRoleMappingEntry(string RoleId, Role Role);