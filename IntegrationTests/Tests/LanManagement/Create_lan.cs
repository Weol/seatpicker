using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Create_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_lan(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_lan_is_valid()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var request = Generator.CreateLanRequest();

        //Act
        var response = await client.PostAsJsonAsync("lan", request);

        //Assert
        var committedAggregates = GetCommittedAggregates<Lan>();

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Created),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Title.Should().Be(request.Title),
                    () => lan.Background.Should().Equal(request.Background));
            });
    }

    [Fact]
    public async Task fails_when_background_is_not_svg()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        //Act
        var response = await client.PostAsJsonAsync(
            "lan",
            Generator.CreateLanRequest() with { Background = new byte[] { 1, 2, 3, 4, 5, 6 } });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}