using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;
using Xunit;

namespace Seatpicker.IntegrationTests.Tests;

public class LoginTest : IntegrationTestBase,  IClassFixture<TestWebApplicationFactory>
{
    public LoginTest(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test1()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = Generator.CreateLan();
        SetupAggregates(existingLan);

        //Act
        var response = await client.PostAsync("lan", Generator.CreateLanRequestModel(existingLan));
        var responseModel = await response.Content.ReadFromJsonAsync<LanController.LanResponseModel>();

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                responseModel.Should().NotBeNull();
                Assert.Multiple(
                    () => responseModel.Id.Should().Be(existingLan.Id),
                    () => responseModel.Title.Should().Be(existingLan.Title),
                    () => responseModel.Background.Should().Be(Convert.ToBase64String(existingLan.Background))
                );
            }
        );
    }

    private static class Generator
    {
        public static Lan CreateLan()
        {
            var background = Convert.FromBase64String(
                "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPCFET0NUWVBFIHN2ZyBQVUJMSUMgIi0vL1czQy8vRFREIFNWRyAxLjEvL0VOIiAiaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkIj4KPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB2ZXJzaW9uPSIxLjEiIHdpZHRoPSI0NDFweCIgaGVpZ2h0PSI4MDFweCIgdmlld0JveD0iLTAuNSAtMC41IDQ0MSA4MDEiIHN0eWxlPSJiYWNrZ3JvdW5kLWNvbG9yOiByZ2IoMjU1LCAyNTUsIDI1NSk7Ij48ZGVmcz48c3R5bGUgdHlwZT0idGV4dC9jc3MiPkBpbXBvcnQgdXJsKGh0dHBzOi8vZm9udHMuZ29vZ2xlYXBpcy5jb20vY3NzP2ZhbWlseT1BcmNoaXRlY3RzK0RhdWdodGVyKTsmI3hhOzwvc3R5bGU+PC9kZWZzPjxnPjxyZWN0IHg9IjAiIHk9IjAiIHdpZHRoPSI0NDAiIGhlaWdodD0iODAwIiBmaWxsPSIjY2NjY2ZmIiBzdHJva2U9Im5vbmUiIHBvaW50ZXItZXZlbnRzPSJhbGwiLz48L2c+PC9zdmc+");

            return new Lan(Guid.NewGuid(), "Test title", background);
        }

        public static HttpContent CreateLanRequestModel(Lan lan)
        {
            var body = JsonSerializer.Serialize(new LanController.CreateLanRequestModel(Id: lan.Id, Title: lan.Title, Background: lan.Background));
            return new StringContent(body);
        }
    }
}