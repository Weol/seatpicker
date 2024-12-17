using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Lan;

public class GuildService(
    IGuildlessDocumentTransaction documentTransaction,
    IGuildlessDocumentReader documentReader,
    IDiscordGuildProvider discordGuildProvider)
{
    public async Task<Guild> Update(Guild guild, User user)
    {
        if (guild.Hostnames.Distinct().Count() != guild.Hostnames.Length)
        {
            throw new DuplicateGuildHostsException(guild.Hostnames);
        }
        
        var duplicateHosts = documentReader.Query<Guild>()
            .Where(document => document.Id != guild.Id)
            .Where(document => document.Hostnames.Any(hostname => guild.Hostnames.Contains(hostname)))
            .AsEnumerable()
            .SelectMany(document => document.Hostnames.Intersect(guild.Hostnames))
            .ToArray();

        if (duplicateHosts.Length > 0)
        {
            throw new DuplicateGuildHostsException(duplicateHosts);
        }

        var discordGuild = await discordGuildProvider.Find(guild.Id) 
            ?? throw new GuildNotFoundException { GuildId = guild.Id };

        var allRolesExist = guild.RoleMapping
            .Select(roleMapping => roleMapping.RoleId)
            .All(roleId => discordGuild.Roles.Any(discordGuildRole => discordGuildRole.Id == roleId));

        if (!allRolesExist)
            throw new IllegalGuildUpdateException("Cannot add role mapping with a discord roel that does not exist");

        var updatedGuild = guild with
        {
            Name = discordGuild.Name,
            Icon = discordGuild.Icon,
            Roles = discordGuild.Roles.Select(role => new GuildRole(role.Id, role.Name, role.Icon)).ToArray()
        };

        documentTransaction.Store(updatedGuild);

        return updatedGuild;
    }
}

/*
 * Exceptions
 */

public class DuplicateGuildHostsException(IEnumerable<string> duplicateHosts) : Exception
{
    public IEnumerable<string> DuplicateHosts { get; init; } = duplicateHosts;
}

public class IllegalGuildUpdateException(string message) : Exception(message);
