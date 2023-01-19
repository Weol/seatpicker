using Microsoft.Extensions.DependencyInjection;
using Seatpicker.SeatContext.Layout.Ports;

namespace Seatpicker.SeatContext.Layout;

public interface ILayoutService
{
    Task<TableLayout> GetActiveLayout();
}

internal class LayoutService : ILayoutService
{
    private static readonly Guid LayoutId = Guid.Parse("cb3e4df0-691a-4057-a207-238417d4e1e5");
    
    private readonly ILookupTables lookupTables;
    private readonly IGetLayoutBackground getLayoutBackground;

    public LayoutService(ILookupTables lookupTables, IGetLayoutBackground getLayoutBackground)
    {
        this.lookupTables = lookupTables;
        this.getLayoutBackground = getLayoutBackground;
    }

    public async Task<TableLayout> GetActiveLayout()
    {
        var (tables, background) = await WhenBoth(lookupTables.Get(LayoutId), getLayoutBackground.Get());

        return new TableLayout(LayoutId, tables, background);
    }

    private async Task<(T1, T2)> WhenBoth<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        await Task.WhenAll(task1, task2);

        return (await task1, await task2);
    }
}

public static class LayoutServiceExtensions
{
    public static IServiceCollection AddLayoutService(this IServiceCollection services)
    {
        return services.AddScoped<ILayoutService, LayoutService>();
    }
}