using Application.DTOs.Admin;
using Application.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
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

        Task<OrderResponse> GetOrderByIdAsync(int orderId);

        //admin sees the orders 

        Task<List<OrderResponse>> GetAllOrdersAsync();

        //admin can set the order status 

        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        //Task UpdateAsync(Order order);

        Task<bool> CancelOrderAsync(int orderId, int userId);

        Task<bool> VerifyOrderPaymentAsync(PaymentVerificationRequest request);

        
        Task<OrderResponse> ProcessDirectBuyAsync(int userId, DirectBuyRequest request);

        Task<DashBoardResponse> GetDashBoardAsync();

        Task<int> CreateReturnRequestAsync(int userId, int orderId, CreateReturnRequest request);
        Task<List<ReturnResponse>> GetAllReturnRequestsAsync();

        Task<int> AddReturnRequestAsync(ReturnRequest request);


    }



    
}
