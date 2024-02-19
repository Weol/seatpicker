using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.IntegrationTests.HttpInterceptor.Discord;

public static class DiscordInterceptorExtensions
{
    public static void AddDiscordInterceptors(this IntegrationTestBase integrationTestBase,
        DiscordUser discordUser,
        string[]? guildRoles = null,
        string? guildNick = null,
        string? guildAvatar = null)
    {
        var accessTokenInterceptor = new AccessTokenInterceptor();
        var refreshTokenInterceptor = new RefreshTokenInterceptor();
        integrationTestBase.AddHttpInterceptor(discordUser);
        integrationTestBase.AddHttpInterceptor(discordUser);
        integrationTestBase.AddHttpInterceptor(new LookupInterceptor(discordUser, accessTokenInterceptor));
        integrationTestBase.AddHttpInterceptor(new GuildMemberInterceptor(discordUser, guildRoleId));
        integrationTestBase.AddHttpInterceptor(new GuildRolesInterceptor(guildRoles, guildRoleId));
    }

    public static void AddDiscordInterceptors(this IntegrationTestBase integrationTestBase,
        DiscordUser discordUser,
        bool isMemberOfGuild)

    {
    }
}