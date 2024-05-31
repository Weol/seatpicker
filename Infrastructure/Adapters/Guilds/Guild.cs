using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters.Guilds;

/**
 * The RoleMappings field will always contain all of the roles that exist in the guild, even roles that
 * have no mappings. To get all the roles of a guild simple use the GuildRole part of each mapping.
 */
public record Guild(
    string Id,
    string Name,
    string? Icon,
    string[] Hostnames,
    (string GuildRoleId, Role[] Roles)[] RoleMapping,
    GuildRole[] Roles);

public record GuildRole(string Id, string Name, int Color, string? Icon);