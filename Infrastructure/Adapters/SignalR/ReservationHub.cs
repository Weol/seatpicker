using Microsoft.AspNetCore.SignalR;

namespace Seatpicker.Infrastructure.Adapters.SignalR;

public class ReservationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext() ?? throw new HttpContextIsNullException();
        var guildId = httpContext.Request.Query["lanId"].FirstOrDefault() ?? throw new LanIdNotPresentException();

        Groups.AddToGroupAsync(Context.ConnectionId, guildId);
        
        return base.OnConnectedAsync();
    }

    public class HttpContextIsNullException : Exception
    {
    }
    
    public class LanIdNotPresentException : Exception
    {
    }
}