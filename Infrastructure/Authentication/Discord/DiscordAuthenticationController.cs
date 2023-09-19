using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Authentication.Discord;

[ApiController]
[Route("discord")]
public class DiscordAuthenticationController : ControllerBase
{
    private readonly DiscordClient.DiscordClient discordClient;
    private readonly DiscordJwtTokenCreator tokenCreator;
    private readonly IDocumentRepository documentRepository;
    private readonly DiscordRoleMapper discordRoleMapper;
    private readonly DiscordAuthenticationOptions options;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;

    public DiscordAuthenticationController(
        DiscordClient.DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator,
        IDocumentRepository documentRepository,
        IOptions<DiscordAuthenticationOptions> options,
        DiscordRoleMapper discordRoleMapper,
        ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.discordClient = discordClient;
        this.tokenCreator = tokenCreator;
        this.documentRepository = documentRepository;
        this.discordRoleMapper = discordRoleMapper;
        this.loggedInUserAccessor = loggedInUserAccessor;
        this.options = options.Value;
    }

    [HttpPost("renew")]
    public async Task<IActionResult> Renew([FromBody] TokenRenewModel model)
    {
        try
        {
            var accessToken = await discordClient.RefreshAccessToken(model.RefreshToken);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return new OkObjectResult(await CreateTokenRequestModel(accessToken, discordUser));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return new BadRequestObjectResult("Token is invalid");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TokenRequestModel model)
    {
        try
        {
            var accessToken = await discordClient.GetAccessToken(model.Token);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return new OkObjectResult(await CreateTokenRequestModel(accessToken, discordUser));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return new BadRequestObjectResult("Token is invalid");
        }
    }

    [HttpGet("test")]
    [Authorize]
    public async Task<IActionResult> Test()
    {
        var roles = HttpContext.User.Identities
            .SelectMany(identity => identity.Claims
                    .Where(claim => claim.Type == identity.RoleClaimType)
                    .Select(claim => claim.Value))
            .ToArray();

        var loggedInuser = loggedInUserAccessor.Get();

        return new OkObjectResult(new TestResponseModel(loggedInuser.Id, loggedInuser.Name, roles));
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roleMappings = await discordRoleMapper.Get(options.GuildId);
        var guildRoles = await discordClient.GetGuildRoles(options.GuildId);

        IEnumerable<DiscordRoleMappingResponseModel> Join()
        {
            foreach (var guildRole in guildRoles)
            {
                Role? role = null;
                foreach (var mapping in roleMappings)
                {
                    if (guildRole.Id == mapping.DiscordRoleId) role = mapping.Role;
                }

                yield return new DiscordRoleMappingResponseModel(
                    guildRole.Id,
                    guildRole.Name,
                    guildRole.Color,
                    guildRole.Icon,
                    role);
            }
        }

        return new OkObjectResult(Join());
    }

    [HttpPut("roles")]
    public async Task<IActionResult> PutRoles([FromBody] DiscordRoleMappingModel model)
    {
        await discordRoleMapper.Set(model.Mappings);
        return new OkResult();
    }

    private async Task<TokenResponseModel> CreateTokenRequestModel(
        DiscordAccessToken accessToken,
        DiscordUser discordUser)
    {
        // Minus 10 just to make sure that the discord token expires a bit after the jwt token actually expires
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(accessToken.ExpiresIn - 10);

        var guildMember = await discordClient.GetGuildMember(options.GuildId, discordUser.Id);
        var roleMapping = await discordRoleMapper.Get(options.GuildId);

        var roles = GetRoles(roleMapping, guildMember).ToArray();

        var token = new DiscordToken(
            discordUser.Id,
            discordUser.Username,
            discordUser.Avatar,
            accessToken.RefreshToken,
            expiresAt);

        var jwtToken = await tokenCreator.CreateToken(token, roles);

        return new TokenResponseModel(jwtToken);
    }

    private IEnumerable<Role> GetRoles(DiscordRoleMapping[] roleMapping, GuildMember guildMember)
    {
        yield return Role.User;

        if (options.Admins.Any(admin => admin == guildMember.DiscordUser.Id)) yield return Role.Admin;

        foreach (var guildRoleId in guildMember.Roles)
        {
            foreach (var mapping in roleMapping)
            {
                if (guildRoleId != mapping.DiscordRoleId) continue;

                yield return mapping.Role;
            }
        }
    }

    public record TokenRequestModel(string Token);

    public record TokenRenewModel(string RefreshToken);

    public record TokenResponseModel(string Token);

    public record TestResponseModel(string? Id, string? Name, string[] Roles);

    public record DiscordRoleMappingModel(DiscordRoleMapping[] Mappings);

    public record DiscordRoleMappingResponseModel(
        string DiscordRoleId,
        string DiscordRoleName,
        int DiscordRoleColor,
        string? DiscordRoleIcon,
        Role? Role);
}