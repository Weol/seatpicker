using Seatpicker.Application.Features.Token.Ports;
using Seatpicker.Domain;

namespace Seatpicker.IntegrationTests;

public record TestIdentity(User User, Role[] Roles, string Token);

public class IdentityGenerator
{
    private readonly IAuthCertificateProvider authCertificateProvider;
    private readonly IJwtTokenCreator jwtTokenCreator;

    public IdentityGenerator(IAuthCertificateProvider authCertificateProvider, IJwtTokenCreator jwtTokenCreator)
    {
        this.authCertificateProvider = authCertificateProvider;
        this.jwtTokenCreator = jwtTokenCreator;
    }

    public async Task<TestIdentity> GenerateWithRoles(params Role[] roles)
    {
        var user = new User { Id = "123456789", Nick = "ToreTang420", Avatar = "123"};

        var certificate = await authCertificateProvider.Get();
        var token = await jwtTokenCreator.CreateFor(user, certificate, roles);

        return new TestIdentity(user, roles, token);
    }
}
