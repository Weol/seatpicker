﻿using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationService(
    DiscordAdapter discordAdapter,
    AuthenticationService authenticationService,
    IGuildlessDocumentReader documentReader)
{
    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken DiscordToken)> Renew(
        string refreshToken,
        string guildId)
    {
        var accessToken = await discordAdapter.RefreshAccessToken(refreshToken);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    public async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken DiscordToken)> Login(
        string discordToken,
        string guildId,
        string redirectUrl)
    {
        var accessToken = await discordAdapter.GetAccessToken(discordToken, redirectUrl);
        var discordUser = await discordAdapter.Lookup(accessToken.AccessToken);

        await discordAdapter.AddGuildMember(guildId, discordUser.Id, accessToken.AccessToken);

        return await CreateToken(accessToken, discordUser, guildId);
    }

    private async Task<(string JwtToken, DateTimeOffset ExpiresAt, AuthenticationToken AuthenticationToken)>
        CreateToken(
            DiscordAccessToken accessToken,
            DiscordUser discordUser,
            string guildId)
    {
        var (roles, guildNick, guildAvatar) = await GetGuildUserInfo(discordUser, guildId);

        return await authenticationService.Login(
            discordUser.Id,
            guildNick ?? discordUser.Username,
            guildAvatar ?? discordUser.Avatar,
            accessToken.RefreshToken,
            roles,
            guildId);
    }

    private async Task<(Role[] Roles, string? Nick, string? Avatar)> GetGuildUserInfo(DiscordUser discordUser,
        string guildId)
    {
        var guildMember = await discordAdapter.GetGuildMember(guildId, discordUser.Id);

        if (guildMember == null) return ([Role.User], discordUser.Username, discordUser.Avatar);

        var guild = await documentReader.Query<Guild>(guildId);

        if (guild is null) throw new DiscordGuildNotFoundException { GuildId = guildId };

        var roles = GetGuildMemberRoles(guild.RoleMapping, guildMember)
            .Append(Role.User)
            .Distinct()
            .ToArray();

        return (roles, guildMember.Nick, guildMember.Avatar);
    }

    private static IEnumerable<Role> GetGuildMemberRoles(GuildRoleMapping[] roleMapping,
        DiscordGuildMember discordGuildMember)
    {
        yield return Role.User;

        foreach (var guildRoleId in discordGuildMember.Roles)
        {
            foreach (var mapping in roleMapping)
            {
                if (guildRoleId != mapping.RoleId) continue;

                foreach (var role in mapping.Roles)
                {
                    yield return role;
                }
            }
        }
    }

    private class DiscordGuildNotFoundException : Exception
    {
        public required string GuildId { get; init; }
    }
}