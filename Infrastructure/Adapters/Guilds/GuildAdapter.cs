using JasperFx.Core;
using Marten;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.Infrastructure.Adapters.Discord;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Guilds;

public class GuildAdapter
{
    private readonly DiscordAdapter discordAdapter;
    private readonly DocumentRepository documentRepository;

    public GuildAdapter(DiscordAdapter discordAdapter, DocumentRepository documentRepository)
    {
        this.discordAdapter = discordAdapter;
        this.documentRepository = documentRepository;
    }

    public async IAsyncEnumerable<Guild> GetGuilds()
    {
        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocuments = reader.Query<GuildDocument>()
            .ToArray();
        
        var discordGuilds = await discordAdapter.GetGuilds();

        foreach (var guild in discordGuilds)
        {
            var roles = await discordAdapter.GetGuildRoles(guild.Id);
            
            var guildDocument = guildDocuments.FirstOrDefault(document => document.Id == guild.Id);

            var hostnames = guildDocument?.Hostnames ?? Array.Empty<string>();
            var roleMappings = guildDocument?.RoleMappings ?? Array.Empty<GuildRoleMapping>();

            var mappings = roleMappings
                .Select(mapping => (mapping.RoleId, mapping.Roles))
                .ToArray();
            
            yield return new Guild(guild.Id,
                guild.Name,
                guild.Icon,
                hostnames,
                mappings,
                roles.Select(GuildRoleFromDiscordGuildRole).ToArray());
        }
    }
    
    public async Task<Guild?> GetGuild(string id)
    {
        var guild = (await discordAdapter.GetGuilds())
            .FirstOrDefault(guild => guild.Id == id);

        var guildRoles = await discordAdapter.GetGuildRoles(id);
        
        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocument = await reader.Query<GuildDocument>(id);

        var hostnames = guildDocument?.Hostnames ?? Array.Empty<string>();
        var roleMappings = guildDocument?.RoleMappings.ToArray() ?? Array.Empty<GuildRoleMapping>();

        if (guild is null) return null;

        return new Guild(guild.Id,
            guild.Name,
            guild.Icon,
            hostnames,
            roleMappings.Select(mapping => (mapping.RoleId, mapping.Roles)).ToArray(),
            guildRoles.Select(GuildRoleFromDiscordGuildRole).ToArray()
        );
    }
    
    public async Task<(string RoleId, Role[] Roles)[]> GetGuildRoleMapping(string id)
    {
        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocument = await reader.Query<GuildDocument>(id);

        if (guildDocument is null) return Array.Empty<(string RoleId, Role[] Roles)>();

        return guildDocument.RoleMappings
            .Select(mapping => (mapping.RoleId, mapping.Roles))
            .ToArray();
    }

    public async Task<string?> GetGuildIdByHost(string host)
    {
        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocument = await reader.Query<GuildDocument>()
            .SingleOrDefaultAsync(document => document.Hostnames.Contains(host));

        if (guildDocument is null) return null;

        return guildDocument.Id;
    }

    public async Task SaveGuild(Guild guild)
    {
        using var reader = documentRepository.CreateGuildlessReader();

        var distinctHosts = guild.Hostnames.Distinct().ToArray();
        if (distinctHosts.Length != guild.Hostnames.Length)
        {
            throw new DuplicateHostsException(guild.Hostnames.Except(distinctHosts));
        }
        
        var duplicateHosts = reader.Query<GuildDocument>()
            .Where(document => document.Id != guild.Id)
            .Where(document => document.Hostnames.Any(hostname => guild.Hostnames.Contains(hostname)))
            .AsEnumerable()
            .SelectMany(document => document.Hostnames.Intersect(guild.Hostnames))
            .ToArray();
        
        if (duplicateHosts.Length > 0)
        {
            throw new DuplicateHostsException(duplicateHosts);
        }

        var guildDocument = GuildDocument.FromGuild(guild);

        using var transaction = documentRepository.CreateGuildlessTransaction();
        transaction.Store(guildDocument);
        await transaction.Commit();
    }

    private static GuildRole GuildRoleFromDiscordGuildRole(DiscordGuildRole role) =>
        new(role.Id, role.Name, role.Color, role.Icon);

    public class DuplicateHostsException : Exception
    {
        public DuplicateHostsException(IEnumerable<string> duplicateHosts)
        {
            DuplicateHosts = duplicateHosts;
        }

        public IEnumerable<string> DuplicateHosts { get; init; }
    }

    public record GuildDocument(
        string Id,
        string[] Hostnames,
        GuildRoleMapping[] RoleMappings) : IDocument
    {
        public static GuildDocument FromGuild(Guild guild)
        {
            var roleMappings = guild.RoleMapping
                .Select(mapping => new GuildRoleMapping(mapping.GuildRoleId, mapping.Roles))
                .ToArray();

            return new GuildDocument(guild.Id,
                guild.Hostnames,
                roleMappings);
        }
    }

    public record GuildRoleMapping(string RoleId, Role[] Roles);
}