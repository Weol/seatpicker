using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class Roles : AuthenticationTestBase
{
    public Roles(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task getting_role_mapping_succeeds()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);
        var guildOperatorRoleId = "1238712";

        var discordAuthenticationOptions = GetService<IOptions<DiscordAuthenticationOptions>>().Value;

        var roleMappings = new DiscordRoleMapper.GuildRoleMapping(
            discordAuthenticationOptions.GuildId,
            new[] { new DiscordRoleMapping(guildOperatorRoleId, Role.Operator) });

        SetupDocuments(roleMappings);
        SetupRolesResponse(guildOperatorRoleId);

        //Act
        var response = await client.GetAsync("discord/roles");
        var Response = await response.Content
            .ReadAsJsonAsync<IEnumerable<DiscordAuthenticationController.DiscordRoleMappingResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Assert.Multiple(
            () =>
            {
                var role = Response.Should().ContainSingle(role => role.DiscordRoleId == guildOperatorRoleId).Subject;
                Assert.Multiple(
                    () => role.DiscordRoleId.Should().Be(guildOperatorRoleId),
                    () => role.Role.Should().Be(Role.Operator));
            });
    }

    [Fact]
    public async Task setting_role_mapping_succeeds()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);
        var guildOperatorRoleId = "1238712";

        var roleMappings = new[] { new DiscordRoleMapping(guildOperatorRoleId, Role.Operator) };

        //Act
        var response = await client.PutAsync(
            "discord/roles",
            JsonContent.Create(new DiscordAuthenticationController.DiscordRoleMappingRequest(roleMappings)));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var roleMapping = GetCommittedDocuments<DiscordRoleMapper.GuildRoleMapping>().Single();
        var discordAuthenticationOptions = GetService<IOptions<DiscordAuthenticationOptions>>().Value;

        Assert.Multiple(
            () => roleMapping.Id.Should().Be(discordAuthenticationOptions.GuildId),
            () =>
            {
                var mapping = roleMapping.Mappings.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => mapping.DiscordRoleId.Should().Be(guildOperatorRoleId),
                    () => mapping.Role.Should().Be(Role.Operator));
            });
    }
}