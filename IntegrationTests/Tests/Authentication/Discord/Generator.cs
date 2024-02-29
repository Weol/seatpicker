using Bogus;
using Seatpicker.Infrastructure.Adapters.Discord;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

public class Generator
{
    public static DiscordUser GenerateDiscordUser()
    {
        return new DiscordUser
        {
            Id = new Faker().Random.Int(1).ToString(),
            Username = new Faker().Name.FirstName(),
            Avatar = new Faker().Random.Int(1).ToString(),
        };
    }
}