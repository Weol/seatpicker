using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly X509Certificate2 certificate;

    public AuthCertificateProvider(
        IOptions<Options> options)
    {
        var bytes = Convert.FromBase64String(options.Value.Base64Certificate);
        certificate = new X509Certificate2(bytes);
    }

    public Task<X509Certificate2> Get()
    {
        return Task.FromResult(certificate);
    }

    internal class Options
    {
        public string Base64Certificate { get; set; } = null!;
    }
}

internal static class AuthenticationCertificateProviderExtensions
{
    internal static IServiceCollection AddAuthCertificateProvider(
        this IServiceCollection services,
        Action<AuthCertificateProvider.Options> configureAction)
    {
        services.Configure(configureAction);

        return services.AddSingleton<IAuthCertificateProvider, AuthCertificateProvider>();
    }
}