using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Application;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Middleware;
using Seatpicker.Infrastructure.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("App_");

if (builder.Environment.IsEnvironment("local"))
{
    builder.Configuration.AddJsonFile("appsettings.local.json");
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

builder.Services
    .AddSingleton(jsonSerializerOptions)
    .AddModelValidator()
    .AddLoggedInUserAccessor()
    .AddAdapters(builder.Configuration)
    .AddApplication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IAuthCertificateProvider authCertificateProvider;

    public ConfigureJwtBearerOptions(IAuthCertificateProvider authCertificateProvider)
    {
        this.authCertificateProvider = authCertificateProvider;
    }

    public void Configure(string name, JwtBearerOptions options)
    {
        // check that we are currently configuring the options for the correct scheme
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            var certificate = authCertificateProvider.Get().GetAwaiter().GetResult();
            var key = new X509SecurityKey(certificate);
            options.TokenValidationParameters.IssuerSigningKey = key;
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        // default case: no scheme name was specified
        Configure(string.Empty, options);
    }
}