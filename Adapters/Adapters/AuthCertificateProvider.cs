﻿using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seatpicker.Domain.Application.UserToken.Ports;

namespace Seatpicker.Adapters.Adapters;

internal class AuthCertificateProvider : IAuthCertificateProvider
{
    private readonly SecretClient secretClient;
    private readonly string secretName;
    
    public AuthCertificateProvider(SecretClient secretClient, IOptions<Options> options)
    {
        this.secretClient = secretClient;
        secretName = options.Value.SecretName;
    }

    public async Task<X509Certificate2> Get()
    {
        var response = await secretClient.GetSecretAsync(secretName);
        var value = response.Value.Value;

        if (value is null) throw new NullReferenceException(nameof(value));
        
        var bytes = Convert.FromBase64String(value);
        return new X509Certificate2(bytes);
    }

    public class Options
    {
        public string SecretName { get; set; } = null!;
        public Uri KeyvaultUri { get; set; } = null!;
    }
}

internal class StaticAuthCertificateProvider : IAuthCertificateProvider
{
    public Task<X509Certificate2> Get()
    {
        var bytes = Convert.FromBase64String(
            "MIIKOQIBAzCCCfUGCSqGSIb3DQEHAaCCCeYEggniMIIJ3jCCBg8GCSqGSIb3DQEHAaCCBgAEggX8MIIF+DCCBfQGCyqGSIb3DQEMCgECoIIE/jCCBPowHAYKKoZIhvcNAQwBAzAOBAib5c+jGc8m5gICB9AEggTYLj1TO0rVUh+UdK0bcbxa+kfK+rmEYoPCyrrYXhzdZHfWpfQTMV1e6QXnRu+9Tdpg3vXW9i4bvXmupepWQEoCs9Gq6Ew3EPeXdeZry0Ds6g9Apr59KtNsfRFPjl3ax9/P0yaqyGPPoH0NnUj8B/3O5smV+w8gUVX5voCFmo2qlsPckv+pwr6v4djXVnsXdSrAMu0OpqpFkeXUb8eg0NEks42Xjj9NiE+iDaV4RByKCuVqOYpE+3iu6bXn0pkSHFi9SdEO28jONnbA+Ah2xOeZ1M7WTjm1DRMoqlfuqEP1RtNBWDjV8hcXwjW/FKUlQ799e1eSKBNUsu7o4CbPK2hgYYGheBx2sux9U7Px0JusazftGBtfAmq83hZIZS3uZ10IiIxcE7ye5t1Wpx7o1YdNoWYFR5XwUwQNy2IcaBPcnDFMzKcwiB7FTyuBc//+ifiDvhIt6Qj8hJ69Ae87ioccdcZOhPoOz+xA9sAdgiQtVX5/yN7M8UuJl5d7fAIvzQ1eJvDfIaZk0Lq6mF/MxmS+4rRgnLQ2cHG8zaVhUBPsNpmvtepfDJLhkJsdoCPTwr+XNoGisWsC91vgmzzepwKpT2x45sN+wH4QxtRq5dqOvJ6ZArCgmlZkf424MqbrvfFpFBXwmpH5URIdoiNNd8k1M7P+ANz/f+fPn6dV/vxlF6ITuD6C/rFq6UJ94pQvAC/ZcaTgnXUEq6QZD+X7accnpQqmj6DgP0gL5dfGRLsJojGxHA9qYM3xyURjddHKqBq9JViA/i32WUtLWsitOCpuKF3n0wFcMkFb6/ACIJqHAiJjSS9IOJsc7Aty/8knrTTGJMjkPxJUfuIGGi60hb3DOdgF13uYZ+b++rytax618KUte+MpYGtymfuSkUeOrJTPUWf3S/tImAOHXcvyVgznxtGTrqgMQvrxybXe9MVeTi0l9MGDMQ3qY8XkypycDpgANvV18ooELIN/mgNvlhkuSoSp9/r5os9xSfxS6jS3K1Og0emmhpYUao5274l2rcaLpPT8cg13FcnjWJcWsi/eynIJFTq8c1eHCUROk9R9PBKgsd3L8ZGNDCEKlMUeSDHqLQqoFnXDMtjv8XG8b8E5ezLkR+ONsOAy4ubm29Zoy4DuoT2cq2UaOhpeEKOh/exiR078K7sXJmx6PKkoY6R2eb9iYBuU47lOqa6wj+xIV6kz2cBHqQeZ63FN6coA0PANUkkPj7DyQQHxXZIItBikICBkNdcEDYbivSWRpS+jtaXbzBuUx+ednxWJ7E5tNECNMTDxbyIlC70R5LrAiHyiJBuKm3OzCn4RQlYATuZMYpUvx2QmZTc3VGSMFXDI1X5ctVMy0icNIhd91stUuW2DQBngj8aJQYGk9CGGAFJhrMDuyB7WD3+YWELWY2sLxf66TY8zp1R0H9sMx67TOEMuXEX61Tef7phcKoTGwdsp3Kl/RipFUsL7cfbkgM6ngl48rsuggRVhw7TsdLTzZpCoa4C+IJWpz+Gld7PPZk/kySS7g4f65o/Vq35ALWL7q4aS6uX9Vk0KZQYPne39qMA7gcp9JFFTpNfunNEQjcCrtWuROAGkQWqZv/SMINSZL5BjR7EVieGdZT3v89H0KE3ALBhMX+skliMbj4kAjhF3E+NvQSoqn5SnHDGB4jANBgkrBgEEAYI3EQIxADATBgkqhkiG9w0BCRUxBgQEAQAAADBdBgkqhkiG9w0BCRQxUB5OAHQAZQAtADQAZAA5ADgANABlADkAOQAtAGUAYgBlADgALQA0AGIAYQBmAC0AYQA4ADAANAAtADQANgAxADIANAAyADMAYgA0AGYAZABiMF0GCSsGAQQBgjcRATFQHk4ATQBpAGMAcgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABvAHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIwggPHBgkqhkiG9w0BBwagggO4MIIDtAIBADCCA60GCSqGSIb3DQEHATAcBgoqhkiG9w0BDAEDMA4ECFKpZEXnwddtAgIH0ICCA4CacFLcl1nl8Ob8lWX9Mz/wmAICziCI2dXYjb3fLcosFGFxrqVJMNMuGGmgz953LJ62GvxJ7N260THBkDFLyeGYM5asAXSz4P0c3NYG6CenDDbmhmLzq6ELP55IMj98GB2Rw0BNtGBlSp1SW5/sASMh2UcXE0vEobuxq2ZBGPcaY3kL5Dyi4/MDhFYOoMU+spfs/hf8dU/CRir7VEgY0xg13lWcRwc9jQil1+rS4eKpD3hrLZfgBbD/rOVGk8bkzLxPyp7/18Beiah9+orLggRyUlRha6B7cVVCvq/F1DLEtcJR6e+1YqilzIXUxTmQ+sVn/E4XvrHC/B6oyQIJmOy1AiZBTapAPO1IyPPvmBAOIfw2oD7eeTkx2p10vK8Y+8VcIVkglQjszm3hG2cGtMKKDT5Ovh/6DlxSNfImhkfgRGxzM8RVzuMHhIUs3jlpwZWlSoRfljmv+tkK8OIFg7xW29el1be3ps4+C4dYT1IHiZ5ee9FYmH4ms1ZrplyHcSy7tnfu4kQRAbj29Zhj59+l0c+gykI/zRHKWayKidWddg+Nr7jyLxPTk01rBSVu7mqPYAv1FmzCDiX7kDeJl7ORazXP0FIGuKUQTTqS79qdM5N1SPPWFwgh/Pg00WxC6885ccY7X02E68pwydECf5mx9Uc0jXOLIpZuX8+UFJM0gMd+/QThNCwioAtdbBbmePwflhN/R1l0ReziRezvqzvkjL3Ttr9U2LaUb0uWj1dQYJ9YyfRuYZV3nVHOoFZPQywMf2uYxqX7Q4GHnexyDhhps7l7CVCE+qD+ZFQxxF/6KY6/seUYKXQj5lvnZ72rhc+N7yUR+fQPJuL1a4KN1+9+6CueoE9vkN+asLWc7939+B1ruwKe1B95OyGmCSU8eTH1DWtqIkAvxGhYnBK4Ov/2hGBEC7lqcXjbMctQUv5x71ac8d5kasR/Iv9NhajF2mT5nYKKphawECyXPhJ/4W1IlUYYqiy0O6UVofPmgejEbVZGbsPvb4Ua1eRpplWVRQXk6b4Du1ZgMetQmIEbm0emMsio6DExiR/FAPzL/ehotmkw6gcrcvHiVDLdAn9IjfmEySDqqEWHOfSpHK2ZLYtLS5cz858IM+ACYOQlTTl7E49FpKAWhK3SHxWBWoYr6/RMRR/DkR69qQ3cjZvP/Z4Bmmb8i4cjUpS2XiZ669UzgjA7MB8wBwYFKw4DAhoEFCoLO04U3Gi2XyWYLZmD+VaNNdSiBBQOJbqaDtVNWo6fsItImcN+oFPDKgICB9A=");

        var certificate = new X509Certificate2(bytes, "asdasd");
        return Task.FromResult(certificate);
    }
}

internal static class AuthenticationCertificateProviderExtensions 
{
    internal static IServiceCollection AddAuthenticationCertificateProvider(this IServiceCollection services, Action<AuthCertificateProvider.Options> configureAction, bool isDevelopment = false)
    {
        return isDevelopment ? services.AddDevelopment() : services.AddProduction();
    }

    private static IServiceCollection AddProduction(this IServiceCollection services)
    {
        services.AddOptions<AuthCertificateProvider.Options>();
        
        services.AddAzureClients(builder =>
        {
            builder.AddClient<SecretClient, AuthCertificateProvider.Options>(options 
                => new SecretClient(options.KeyvaultUri, new DefaultAzureCredential()));
        });
        
        return services.AddScoped<IAuthCertificateProvider, AuthCertificateProvider>();
    }
    
    private static IServiceCollection AddDevelopment(this IServiceCollection services)
    {
        return services.AddSingleton<IAuthCertificateProvider, StaticAuthCertificateProvider>();
    }
}