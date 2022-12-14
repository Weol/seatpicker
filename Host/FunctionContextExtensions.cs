using Microsoft.Azure.Functions.Worker;

namespace Seatpicker.Host;

public static class FunctionContextExtensions
{
    public static bool IsHttpTrigger(this FunctionContext context)
    {
         return context.FunctionDefinition.InputBindings.Values
            .Any(a => a.Type == "httpTrigger");
    }
}