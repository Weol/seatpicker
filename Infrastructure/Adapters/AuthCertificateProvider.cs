﻿using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Login.Ports;

namespace Seatpicker.Infrastructure.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly Options options;

    public AuthCertificateProvider(
        IOptions<Options> options)
    {
        this.options = options.Value;
    }

    public Task<X509Certificate2> Get()
    {
        var bytes = Convert.FromBase64String(options.Base64Certificate);
        return Task.FromResult(new X509Certificate2(bytes, "", X509KeyStorageFlags.MachineKeySet));
    }

    internal class Options
    {
        [Required]
        [RegularExpression("[a-zA-Z0-9+/]+={0,2}")]
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
