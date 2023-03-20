using Microsoft.Extensions.Azure;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters.AuthCertificateProvider;

internal class AuthCertificateProviderOptions
{
    public string SecretName { get; set; } = null!;
    public Uri Uri { get; set; } = null!;
    public bool IsFake { get; set; }
}

internal static class AuthenticationCertificateProviderExtensions
{
    internal static IServiceCollection AddAuthCertificateProvider(this IServiceCollection services,
        Action<AuthCertificateProviderOptions> configureAction)
    {
        var options = new AuthCertificateProviderOptions();
        configureAction(options);

        if (options.IsFake) return services.AddFake();

        return services.AddReal(configureAction);
    }

    private static IServiceCollection AddReal(this IServiceCollection services, Action<AuthCertificateProviderOptions> configureAction)
    {
        services.Configure(configureAction);

        services.AddAzureClients(
            builder =>
            {
                var options = new AuthCertificateProviderOptions();
                configureAction(options);

                builder.AddSecretClient(options.Uri).WithName(options.Uri.ToString());
            });

        return services.AddSingleton<IAuthCertificateProvider, AuthCertificateProvider>();
    }

    private static IServiceCollection AddFake(this IServiceCollection services)
    {
        return services.AddSingleton<IAuthCertificateProvider, FakeAuthCertificateProvider>();
    }
}