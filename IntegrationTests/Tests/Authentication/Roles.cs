using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public class Roles : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
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
        var client = GetAnonymousClient();
        var guildOperatorRoleId = "1238712";

        var discordAuthenticationOptions = GetService<IOptions<DiscordAuthenticationOptions>>().Value;

        var roleMappings = new DiscordRoleMapper.GuildRoleMapping(
            discordAuthenticationOptions.GuildId,
            new[] { new DiscordRoleMapping(guildOperatorRoleId, Role.Operator) });

        SetupDocuments(roleMappings);

        this.SetupRolesResponse(guildOperatorRoleId);

        //Act
        var response = await client.GetAsync("discord/roles");
        var responseModel = await response.Content
            .ReadFromJsonAsync<DiscordAuthenticationController.DiscordRoleMappingResponseModel[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Assert.Multiple(
            () =>
            {
                var role = responseModel.Should().Contain(role => role.DiscordRoleId == guildOperatorRoleId).Subject;
                Assert.Multiple(
                    () => role.DiscordRoleId.Should().Be(guildOperatorRoleId),
                    () => role.Role.Should().Be(Role.Operator));
            });
    }

    [Fact]
    public async Task setting_role_mapping_succeeds()
    {
        // Arrange
        var client = GetAnonymousClient();
        var guildOperatorRoleId = "1238712";

        var roleMappings = new[] { new DiscordRoleMapping(guildOperatorRoleId, Role.Operator) };

        //Act
        var response = await client.PutAsync(
            "discord/roles",
            JsonContent.Create(new DiscordAuthenticationController.DiscordRoleMappingRequestModel(roleMappings)));

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