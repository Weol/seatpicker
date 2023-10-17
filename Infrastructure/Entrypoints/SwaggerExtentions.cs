using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(
            options =>
            {
                options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
                options.OperationFilter<CustomOperationFilter>();
                options.CustomSchemaIds(CreateCustomSchemeId);


                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Bearer token authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Bearer token acquired through the Discord authentication endpoints",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme,
                    },
                };

                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
            });

        return services;
    }

    public static IApplicationBuilder UseSwaggerGen(this IApplicationBuilder app)
    {
        return app
            .UseSwagger()
            .UseSwaggerUI();
    }

    private static string CreateCustomSchemeId(Type type)
    {
        if (type.IsNested)
        {
            var declaringType = type.DeclaringType!;

            var httpNamespace = typeof(EntrypointsExtensions).Namespace! + "." + nameof(Http);
            if (declaringType.Namespace!.StartsWith(httpNamespace))
            {
                return declaringType.Name.Replace("Endpoint", "") + declaringType.Namespace.Substring(httpNamespace.Length + 1).Replace(".", "") + type.Name;
            }
        }

        return type.Name;
    }

    public class CustomOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var type = context.MethodInfo.DeclaringType;

            string? name = null;
            foreach (Attribute attr in type!.GetCustomAttributes(false))
            {
                if (attr is AreaAttribute areaAttribute)
                {
                    name = areaAttribute.RouteValue;
                }
            }

            if (name is not null)
            {
                operation.OperationId = context.MethodInfo.Name + char.ToUpper(name[0]) + name.Substring(1);

                operation.Tags.Clear();
                operation.Tags.Add(new OpenApiTag
                {
                    Name = name,
                });
            }
        }
    }
}