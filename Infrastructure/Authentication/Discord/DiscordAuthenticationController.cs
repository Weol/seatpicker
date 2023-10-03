using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly UserManager userManager;

    public DiscordAuthenticationController(
        DiscordClient.DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator,
        IDocumentRepository documentRepository,
        IOptions<DiscordAuthenticationOptions> options,
        DiscordRoleMapper discordRoleMapper,
        ILoggedInUserAccessor loggedInUserAccessor,
        UserManager userManager)
    {
        this.discordClient = discordClient;
        this.tokenCreator = tokenCreator;
        this.documentRepository = documentRepository;
        this.discordRoleMapper = discordRoleMapper;
        this.loggedInUserAccessor = loggedInUserAccessor;
        this.userManager = userManager;
        this.options = options.Value;
    }

    [HttpPost("renew")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Renew([FromBody] RenewRequest Request)
    {
        try
        {
            var accessToken = await discordClient.RefreshAccessToken(Request.RefreshToken);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return new OkObjectResult(await CreateTokenRequest(accessToken, discordUser));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return new BadRequestObjectResult("Token is invalid");
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        try
        {
            var accessToken = await discordClient.GetAccessToken(model.Token);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return new OkObjectResult(await CreateTokenRequest(accessToken, discordUser));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return new BadRequestObjectResult("Token is invalid");
        }
    }

    [Authorize]
    [HttpGet("test")]
    [ProducesResponseType(typeof(TestResponse), 200)]
    public async Task<IActionResult> Test()
    {
        var roles = HttpContext.User.Identities
            .SelectMany(identity => identity.Claims
                    .Where(claim => claim.Type == identity.RoleClaimType)
                    .Select(claim => claim.Value))
            .ToArray();

        var loggedInuser = await loggedInUserAccessor.Get();

        return new OkObjectResult(new TestResponse(loggedInuser.Id, loggedInuser.Name, roles));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("roles")]
    [ProducesResponseType(typeof(DiscordRoleMappingResponse[]), 200)]
    public async Task<IActionResult> GetRoles()
    {
        var roleMappings = await discordRoleMapper.Get(options.GuildId);
        var guildRoles = await discordClient.GetGuildRoles(options.GuildId);

        IEnumerable<DiscordRoleMappingResponse> Join()
        {
            foreach (var guildRole in guildRoles)
            {
                Role? role = null;
                foreach (var mapping in roleMappings)
                {
                    if (guildRole.Id == mapping.DiscordRoleId) role = mapping.Role;
                }

                yield return new DiscordRoleMappingResponse(
                    guildRole.Id,
                    guildRole.Name,
                    guildRole.Color,
                    guildRole.Icon,
                    role);
            }
        }

        return new OkObjectResult(Join());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("roles")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> PutRoles([FromBody] DiscordRoleMappingRequest Request)
    {
        await discordRoleMapper.Set(options.GuildId, Request.Mappings);
        return new OkResult();
    }

    private async Task<TokenResponse> CreateTokenRequest(
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
        await userManager.Store(new User(new UserId(discordUser.Id), discordUser.Username, discordUser.Avatar));

        return new TokenResponse(jwtToken, discordUser.Id, discordUser.Username, discordUser.Avatar, roles);
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

    public record LoginRequest(string Token);

    public record RenewRequest(string RefreshToken);

    public record TokenResponse(string Token, string UserId, string Nick, string? Avatar, ICollection<Role> Roles);

    public record TestResponse(string? Id, string? Name, string[] Roles);

    public record DiscordRoleMappingRequest(DiscordRoleMapping[] Mappings);

    public record DiscordRoleMappingResponse(
        string DiscordRoleId,
        string DiscordRoleName,
        int DiscordRoleColor,
        string? DiscordRoleIcon,
        Role? Role);
}