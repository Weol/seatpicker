using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

namespace Seatpicker.Infrastructure;

public static class SeatpickerConfigurationBuilderExtensions
{
    public static ConfigurationManager AddSeatpickerKeyvault(this ConfigurationManager configurationManager)
    {
        var section = configurationManager.GetSection("Keyvault");

        if (section["TenantId"] is not null && section["ClientSecret"] is not null && section["ClientId"] is not null)
        {
            configurationManager.AddAzureKeyVault(
                section.GetValue<Uri>("Uri"),
                new ClientSecretCredential(section["TenantId"], section["ClientId"], section["ClientSecret"]),
                new AzureKeyVaultConfigurationOptions
                {
                    ReloadInterval = TimeSpan.FromSeconds(5),
                });
        }
        else if (section["Uri"] is not null)
        {
            configurationManager.AddAzureKeyVault(section.GetValue<Uri>("Uri"), new ManagedIdentityCredential());
        }

        return configurationManager;
    }
}