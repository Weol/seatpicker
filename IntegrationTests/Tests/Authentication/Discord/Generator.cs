using Bogus;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

public class Generator
{
    public static DiscordUser GenerateDiscordUser()
    {
        return new DiscordUser(new Faker().Random.Int(1).ToString(),
            new Faker().Name.FirstName(),
            new Faker().Random.Int(1).ToString());
    }
}