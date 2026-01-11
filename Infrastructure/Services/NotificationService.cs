using Application.Contracts.Services;
using Domain.Entities;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task OrderPlacedAsync(string message, int orderId)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveOrderNotification",
                message,
                orderId
            );
        }
    }
}
