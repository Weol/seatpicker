using Bogus;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;

namespace Seatpicker.IntegrationTests.Tests.Guild;

public static class Generator
{
    public static UpdateGuild.Request UpdateGuildRequest()
    {
        var faker = new Faker();
        return new UpdateGuild.Request(
            faker.Random.Int(9999, 999999999).ToString(),
            faker.Company.CompanyName(),
            null,
            faker.Make(2, () => faker.Internet.DomainName()).ToArray(),
            Array.Empty<(string RoleId, Role[] Roles)>());
    }
}