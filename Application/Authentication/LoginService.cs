using Microsoft.Extensions.DependencyInjection;

namespace Application.Authentication;

public interface ILoginTokenService
{
    public string GetTokenFor(string id, string password);
}

internal class LoginTokenService : ILoginTokenService
{
    public string GetTokenFor(string id, string password)
    {
        return "<jwt token>";
    }

    public class Options
    {
        public string Greeting { get; set; }
    }
}

internal static class LoginServiceExtensions
{
    public static IServiceCollection AddLoginService(this IServiceCollection services, Action<LoginTokenService.Options> configureAction)
    {
        services.Configure(configureAction);
        
        return services.AddSingleton<ILoginTokenService, LoginTokenService>();
    }
}
