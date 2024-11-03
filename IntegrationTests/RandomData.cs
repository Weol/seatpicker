using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Bogus;
using Marten.Internal.CodeGeneration;
using Microsoft.AspNetCore.Builder;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Discord;
using Seatpicker.Infrastructure.Adapters.Guilds;

namespace Seatpicker.IntegrationTests;

public static class RandomData
{
    public static readonly Faker Faker = new();

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static T NotAnyOf<T>(IEnumerable<T> any, Func<T> func) where T : notnull
    {
        T t;
        do { 
            t = func();
        } while (any.Contains(t));

        return t;
    }

    public static string NumericId() => Faker.Random.Int(10000, 999999).ToString(CultureInfo.InvariantCulture);
    private static int DiscordColor() => Faker.Random.Int(10000, 999999);
    public static string Hostname() => Faker.Internet.DomainName();

    public static DiscordUser DiscordUser() => new(NumericId(), Faker.Name.FirstName(), null, NumericId());

    public static DiscordGuild DiscordGuild() => new(NumericId(), Faker.Company.CompanyName(), NumericId());

    public static DiscordGuildRole DiscordGuildRole() =>
        new(NumericId(), Faker.Name.JobType(), DiscordColor(), NumericId());

    public static Guild Guild()
    {
        var roles = new[] { GuildRole(), GuildRole() };
        return new Guild(NumericId(),
            Faker.Company.CompanyName(),
            NumericId(),
            new[] { Hostname(), Hostname() },
            new[] { (roles[0].Id, new[] { Role.Operator }) },
            roles
        );
    }

    public static GuildRole GuildRole() => new(NumericId(), Faker.Name.JobType(), DiscordColor(), NumericId());

    public static User User() => new(NumericId(), Faker.Name.FirstName(), NumericId(), Array.Empty<Role>());

    public static class Aggregates
    {
        public static byte[] LanBackground()
        {
            var svg = $"<svg>{Random.Shared.NextInt64()}</svg>";
            return Encoding.UTF8.GetBytes(svg);
        }

        public static Lan Lan(string guildId,
            User actor,
            Guid? id = null,
            string? title = null,
            byte[]? background = null)
        {
            return new Lan(
                id ?? Guid.NewGuid(),
                guildId,
                title ?? "Test title",
                background ?? LanBackground(),
                actor);
        }
    }
}