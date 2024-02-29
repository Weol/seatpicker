using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
public class Roles : IntegrationTestBase
{
    public Roles(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task getting_role_mapping_succeeds()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Admin);
        var guildRoleId = "1238712";

        var roleMapping =
            new GuildRoleMapping(guildId, new[] { new GuildRoleMappingEntry(guildRoleId, Role.Operator) });
        await SetupDocuments(guildId, roleMapping);
        
        //Act
        var response = await client.GetAsync($"guild/{guildId}/roles");
        var body = await response.Content.ReadAsJsonAsync<IEnumerable<GetRoleMapping.Response>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Assert.Multiple(
            () =>
            {
                var mapping = body.Should().ContainSingle(role => role.Id == guildRoleId).Subject;
                mapping.Roles.Should().ContainSingle(role => role == Role.Operator);
            });
    }

    [Fact]
    public async Task setting_role_mapping_succeeds()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Admin);
        var guildRoleId1 = "1238712";
        var guildRoleId2 = "1238712233";

        var request = new PutRoleMapping.Request[]
        {
            new(guildRoleId1, new[] { Role.Operator }),
            new(guildRoleId2, new[] { Role.Operator, Role.Admin }),
        };

        //Act
        var response = await client.PutAsync(
            $"guild/{guildId}/roles",
            JsonContent.Create(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var roleMappings = GetCommittedDocuments<GuildRoleMapping>(guildId);

        Assert.Multiple(
            () => roleMappings.Should().HaveCount(1),
            () =>
            {
                var mapping = roleMappings.Should().ContainSingle(mapping => mapping.GuildId == guildId).Subject;
                Assert.Multiple(
                    () => mapping.Mappings.Should().HaveCount(3),
                    () => mapping.Mappings.Should().ContainSingle(entry =>
                        entry.RoleId == guildRoleId1 && entry.Role == Role.Operator),
                    () => mapping.Mappings.Should().ContainSingle(entry =>
                        entry.RoleId == guildRoleId2 && entry.Role == Role.Operator),
                    () => mapping.Mappings.Should().ContainSingle(entry =>
                        entry.RoleId == guildRoleId2 && entry.Role == Role.Admin)
                );
            });
    }
}