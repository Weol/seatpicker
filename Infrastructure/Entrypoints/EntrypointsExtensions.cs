using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Seatpicker.Infrastructure.Middleware;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class EntrypointsExtensions
{
    public static IServiceCollection AddEntrypoints(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddModelValidator()
            .AddBus(GetMassTransitOptions(configuration))
            .AddHealthChecks()
            .Services.AddControllers(ConfigureMvcOptions);

        return services;
    }

    private static void ConfigureMvcOptions(MvcOptions options)
    {
        options.UseRoutePrefix("api");
        options.Filters.Add<HttpResponseExceptionFilter>();
    }

    private static MassTransitOptions GetMassTransitOptions(IConfiguration configuration)
    {
        var options = new MassTransitOptions();
        configuration.GetSection("MassTransit").Bind(options);
        return options;
    }
}

public static class MvcOptionsExtensions
{
    public static void UseRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
    {
        opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
    }

    public static void UseRoutePrefix(this MvcOptions opts, string
        prefix)
    {
        opts.UseRoutePrefix(new RouteAttribute(prefix));
    }
}
public class RoutePrefixConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel routePrefix;

    public RoutePrefixConvention(IRouteTemplateProvider route)
    {
        routePrefix = new AttributeRouteModel(route);
    }
    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
        {
            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(routePrefix, selector.AttributeRouteModel);
            }
            else
            {
                selector.AttributeRouteModel = routePrefix;
            }
        }
    }
}