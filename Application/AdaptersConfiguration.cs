using Microsoft.Extensions.Configuration;

namespace Seatpicker.Adapters;

public class ApplicationConfiguration
{
    public static ApplicationConfiguration From(IConfiguration config)
    {
        return new ApplicationConfiguration(config);
    }

    private ApplicationConfiguration(IConfiguration config)
    {
        UserLoginGreetingMessage = config["UserLoginGreetingMessage"];
    }

    public virtual string UserLoginGreetingMessage { get; }
}