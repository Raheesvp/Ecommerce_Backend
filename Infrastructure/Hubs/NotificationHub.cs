using Microsoft.AspNetCore.SignalR;
using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {

        public async Task SendOrderNotification(string message, int orderId)
        {
            await Clients.All.SendAsync("Receive Order Notification", message, orderId);

        }

    }
}
