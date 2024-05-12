using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters.Guilds;

public record Guild(
    string Id,
    string Name,
    string? Icon,
    string[] Hostnames,
    (string RoleId, Role[] Roles)[] RoleMapping);