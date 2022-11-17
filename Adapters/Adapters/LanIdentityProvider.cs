using Application.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Seatpicker.Adapters.Adapters;

internal class LanIdentityProvider : ILanIdentityProvider
{
    private readonly Options options;

    public LanIdentityProvider(IOptions<Options> options)
    {
        this.options = options.Value;
    }

    public Task<Guid> GetCurrentLanId()
    {
        return Task.FromResult(options.LanId);
    }

    public class Options
    {
        public Guid LanId { get; set; }
    }
}

internal static class LanIdentityProviderExtensions
{
    public static IServiceCollection AddLanIdentityProvider(this IServiceCollection services, Action<LanIdentityProvider.Options> configureAction)
    {
        services.Configure(configureAction);
        
        return services.AddSingleton<ILanIdentityProvider, LanIdentityProvider>();
    }
}
