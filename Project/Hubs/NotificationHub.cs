using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Mscc.GenerativeAI;

namespace Domain.Entities
{
    public  class NotificationHub :  Hub
    {

        public async Task SendOrderNotification(string message,int orderId)
        {
            await Clients.All.SendAsync("Receive Order Notification", message, orderId);
            
        }

    }
}
