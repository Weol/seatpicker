using Seatpicker.Domain;
using Shared;

namespace Seatpicker.Application.Features.Lan;

public record Guild(string Id, string Name, string? Icon, string[] Hostnames, GuildRoleMapping[] RoleMapping)
    : IDocument;

public record GuildRoleMapping(string RoleId, Role[] Roles);