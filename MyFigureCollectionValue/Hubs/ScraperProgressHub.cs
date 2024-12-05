using Microsoft.AspNetCore.SignalR;

namespace MyFigureCollectionValue.Hubs
{
    public class ScraperProgressHub : Hub
    {
        public async Task UpdateProgress(int current, int total, string status)
        {
            await Clients.All.SendAsync("ReceiveProgress", current, total, status);
        }
    }
}
