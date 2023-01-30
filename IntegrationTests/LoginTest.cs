using Xunit;

namespace Seatpicker.IntegrationTests;

public class LoginTest
{
    [Fact]
    public async Task Test1()
    {
        var host = new Host.Host();
    }

    private record LoginRequestModel(string Token);

    private record LoginResponseModel(string Token);
}