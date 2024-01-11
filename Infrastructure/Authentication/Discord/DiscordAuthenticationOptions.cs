using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public class DiscordAuthenticationOptions
{
    [Required]
    public string Base64SigningCertificate { get; set; } = null!;
    
    public X509Certificate2 SigningCertificate =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 
            new (Convert.FromBase64String(Base64SigningCertificate)) : 
            new (Convert.FromBase64String(Base64SigningCertificate), "", X509KeyStorageFlags.MachineKeySet);

    [Required]
    public int TokenLifetime { get; set; }

    [Required]
    public string[] Superadmins { get; set; } = null!;
}