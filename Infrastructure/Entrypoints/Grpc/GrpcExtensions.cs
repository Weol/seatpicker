using Seatpicker.Infrastructure.Entrypoints.Grpc.Frontend;

namespace Seatpicker.Infrastructure.Entrypoints.Grpc;

public static class GrpcExtensions
{
    public static IServiceCollection AddGrpc(this IServiceCollection services)
    {
        return services
            .AddGrpc()
            .AddSingleton<FrontendService>();
    }
}