using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebTvApiService.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTvApiService.SignalR
{
    [Authorize]
    public class CommandHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("CommandHub OnConnectedAsync");
                await base.OnConnectedAsync();
                await Clients.All.SendAsync($"ReceiveRandomNumber", "connect");

            }
            catch (Exception ex) { 
                Console.WriteLine($"CommandHub OnConnectedAsync = {ex.Message}");
            }
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync($"ReceiveRandomNumber", "disconnect");
            Console.WriteLine("ON_DISCONNECTED_ASYNC");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddMessageToChat(string user, string message)
        {
            Console.WriteLine("CommandHub AddMessageToChat");
            Console.WriteLine($"CommandHub {user} {message}");
            //            await Clients.All.SendAsync($"ReceiveRandomNumber", "ToChat");
            await Clients.All.SendAsync("AddMessageToChat2", "message from service");

        }

        public async Task SendMessage(string user, string message)
        {
            var userName = Context?.User?.FindFirstValue(ClaimTypes.Name) ?? "anonymous";
            
            Console.WriteLine($"CommandHub ReceiveMessage {userName}");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
