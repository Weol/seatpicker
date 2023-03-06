using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .PostConfigure<IAuthCertificateProvider>(
        (options, provider)  =>
        {
            var certificate = provider.Get().GetAwaiter().GetResult();
            var key = new X509SecurityKey(certificate);

            options.TokenValidationParameters.IssuerSigningKey = key;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidIssuer = certificate.Thumbprint;
        });

builder.Services
    .AddAuthorization()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();