using Microsoft.Extensions.Configuration;

namespace Seatpicker.Adapters;

public class AdaptersConfiguration
{
    public static AdaptersConfiguration From(IConfiguration config)
    {
        return new AdaptersConfiguration(config);
    }

    private AdaptersConfiguration(IConfiguration config)
    {
        AuthenticationCertificateSecretName = config["AuthenticationCertificateSecretName"];
        KeyvaultUri = new Uri(config["KeyvaultUri"]);
        TableStorageUri = new Uri(config["StorageEndpoint"]);
    }

    public virtual string AuthenticationCertificateSecretName { get; }
    public virtual Uri KeyvaultUri { get; }
    public virtual Uri TableStorageUri { get; }
}