using Microsoft.AspNetCore.SignalR;

namespace WebTvApiService.SignalR
{
    public class TvHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).ReceiveNotification($"Thank you connecting {Context.User?.Identity?.Name}");
            await base.OnConnectedAsync();
        }

    }

    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
    }
}
