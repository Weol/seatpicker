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
        var discordGuilds = (await discordAdapter.GetGuilds())
            .ToArray();

        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocuments = reader.Query<GuildDocument>()
            .ToArray();

        foreach (var discordGuild in discordGuilds)
        {
            var guildDocument = guildDocuments.FirstOrDefault(document => document.Id == discordGuild.Id);

            var hostnames = guildDocument?.Hostnames ?? Array.Empty<string>();
            var roleMappings = guildDocument?.RoleMappings ?? Array.Empty<GuildRoleMapping>();

            yield return new Guild(discordGuild.Id,
                discordGuild.Name,
                discordGuild.Icon,
                hostnames,
                roleMappings.Select(mapping => (mapping.RoleId, mapping.Roles)).ToArray());
        }
    }

    public async Task<Guild?> GetGuild(string id)
    {
        var guild = (await discordAdapter.GetGuilds())
            .FirstOrDefault(guild => guild.Id == id);

        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocument = await reader.Get<GuildDocument>(id);

        var hostnames = guildDocument?.Hostnames ?? Array.Empty<string>();
        var roleMappings = guildDocument?.RoleMappings.ToArray() ?? Array.Empty<GuildRoleMapping>();

        if (guild is null) return null;

        return new Guild(guild.Id,
            guild.Name,
            guild.Icon,
            hostnames,
            roleMappings.Select(mapping => (mapping.RoleId, mapping.Roles)).ToArray()
        );
    }

    public async Task<Guild?> GetGuildByHost(string host)
    {
        using var reader = documentRepository.CreateGuildlessReader();
        var guildDocument = await reader.Query<GuildDocument>()
            .SingleOrDefaultAsync(document => document.Hostnames.Contains(host));

        return guildDocument != null
            ? (Guild)new(guildDocument.Id,
                guildDocument.Name,
                guildDocument.Icon,
                guildDocument.Hostnames,
                guildDocument.RoleMappings.Select(mapping => (mapping.RoleId, mapping.Roles)).ToArray())
            : null;
    }

    public async Task<Guild> SaveGuild(Guild guild)
    {
        var discordGuild = (await discordAdapter.GetGuilds())
            .First(discordGuild => discordGuild.Id == guild.Id);

        using var reader = documentRepository.CreateGuildlessReader();

        var duplicateHosts = reader.Query<GuildDocument>()
            .Where(document => document.Hostnames.Any(hostname => guild.Hostnames.Contains(hostname)))
            .AsEnumerable()
            .SelectMany(document => document.Hostnames.Intersect(guild.Hostnames))
            .ToArray();

        if (duplicateHosts.Length > 0)
        {
            throw new DuplicateHostsException(duplicateHosts);
        }

        var guildDocument = GuildDocument.FromGuild(guild) with
        {
            Name = discordGuild.Name,
            Icon = discordGuild.Icon
        };

        using var transaction = documentRepository.CreateGuildlessTransaction();
        transaction.Store(guildDocument);
        await transaction.Commit();

        return new Guild(guildDocument.Id,
            guildDocument.Name,
            guildDocument.Icon,
            guildDocument.Hostnames,
            guildDocument.RoleMappings.Select(mapping => (mapping.RoleId, mapping.Roles)).ToArray());
    }

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
        string Name,
        string? Icon,
        string[] Hostnames,
        GuildRoleMapping[] RoleMappings) : IDocument
    {
        public static GuildDocument FromGuild(Guild guild)
        {
            var roleMappings = guild.RoleMapping
                .Select(mapping => new GuildRoleMapping(mapping.RoleId, mapping.Roles))
                .ToArray();

            return new GuildDocument(guild.Id,
                guild.Name,
                guild.Icon,
                guild.Hostnames,
                roleMappings);
        }
    }

    public record GuildRoleMapping(string RoleId, Role[] Roles);
}