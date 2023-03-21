using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly Options options;

    public AuthCertificateProvider(IOptions<Options> options)
    {
        this.options = options.Value;
    }

    public Task<X509Certificate2> Get()
    {
        throw new Exception("Base64: " + options.Base64Certificate);
        var bytes = Convert.FromBase64String(options.Base64Certificate);
        return Task.FromResult(new X509Certificate2(bytes, string.Empty, X509KeyStorageFlags.MachineKeySet));
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
