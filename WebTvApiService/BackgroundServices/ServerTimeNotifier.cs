
using Microsoft.AspNetCore.SignalR;
using WebTvApiService.SignalR;

namespace WebTvApiService.BackgroundServices
{
    public class ServerTimeNotifier : BackgroundService
    {
        private static readonly TimeSpan Period = TimeSpan.FromSeconds(1);
        private readonly ILogger<ServerTimeNotifier> _logger;
        private readonly IHubContext<TvHub, INotificationClient> _hubContext;
        private readonly IHubContext<CommandHub> _commandHub;

        public ServerTimeNotifier(ILogger<ServerTimeNotifier> logger, IHubContext<TvHub, INotificationClient> hubContext, IHubContext<CommandHub> commandHub)
        {
            Console.WriteLine("ServerTimeNotifier");
            _logger = logger;
            _hubContext = hubContext;
            _commandHub = commandHub;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Period);
            while (!stoppingToken.IsCancellationRequested && 
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                var dateTime = DateTime.Now;
                _logger.LogInformation("Executing {Service} {Time}", nameof(ServerTimeNotifier), dateTime);
                await _hubContext.Clients.All.ReceiveNotification($"Server time = {dateTime}");
                await _commandHub.Clients.All.SendAsync("AddMessageToChat2", "background", $"Server time = {dateTime}");
                await _commandHub.Clients.All.SendAsync("Timer", $"Server time = {dateTime}");
            }
        }
    }
}
