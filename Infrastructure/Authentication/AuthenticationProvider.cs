namespace Seatpicker.Infrastructure.Authentication;

public class AuthenticationProvider
{
    public static readonly AuthenticationProvider Discord = new("D_");
    
    public string Prefix { get; }

    private AuthenticationProvider(string prefix)
    {
        Prefix = prefix;
    }
}