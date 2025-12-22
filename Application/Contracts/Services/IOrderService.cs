using Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public  interface IOrderService
    {
        Task<int> PlaceOrderAsync(int userId, CreateOrderRequest orderRequest);

        Task<List<OrderResponse>> GetUserOrdersAsync(int userId);

        //get the order by id 

        Task<OrderResponse> GetOrderByIdAsync(int userId, int orderId);
    }

    
}
