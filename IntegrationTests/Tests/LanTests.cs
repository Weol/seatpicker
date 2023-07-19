using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Seatpicker.Application.Features.Login;
using Seatpicker.Application.Features.Login.Ports;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public class LoginTest : IntegrationTestBase,  IClassFixture<TestWebApplicationFactory>
{
    public LoginTest(TestWebApplicationFactory factory, ITestOutputHelper outputHelper) : base(factory, outputHelper)
    {

    }

    [Fact]
    public async Task Test1()
    {

    }
}