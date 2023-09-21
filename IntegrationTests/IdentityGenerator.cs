using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;

namespace Seatpicker.IntegrationTests;

public record TestIdentity(Domain.User User, Role[] Roles, string Token);

public class IdentityGenerator
{
    private readonly DiscordJwtTokenCreator jwtTokenCreator;

    public IdentityGenerator(DiscordJwtTokenCreator jwtTokenCreator)
    {
        this.jwtTokenCreator = jwtTokenCreator;
    }

    public async Task<TestIdentity> GenerateWithRoles(params Role[] roles)
    {
        var discordToken = new DiscordToken(
            Id: "123",
            Nick: "Tore Tang",
            RefreshToken: "8ioq3",
            ExpiresAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            Avatar: null
        );

        var token = await jwtTokenCreator.CreateToken(discordToken, roles);

        var user = new Domain.User(new UserId(discordToken.Id), discordToken.Nick);
        return new TestIdentity(user, roles, token);
    }
}
