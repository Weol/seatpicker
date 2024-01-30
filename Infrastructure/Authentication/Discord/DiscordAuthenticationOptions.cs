using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationOptions
{
    [Required] public string? Base64SigningCertificate { get; set; } = null!;

    public X509Certificate2 SigningCertificate
    {
        get
        {
            var base64 = Base64SigningCertificate ??
                         throw new ArgumentNullException("Base64Certificate cannot be null");

            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? new (Convert.FromBase64String(base64))
                : new (Convert.FromBase64String(base64), "", X509KeyStorageFlags.MachineKeySet);
        }
    }

    [Required] public int TokenLifetime { get; set; }

    [Required] public string[] Superadmins { get; set; } = null!;
}