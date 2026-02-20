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
        // USER – PLACE ORDER
        Task<int> PlaceOrderAsync(int userId, CreateOrderRequest orderRequest);

        // USER – VIEW ORDERS
        Task<List<OrderResponse>> GetUserOrdersAsync(int userId);
        Task<OrderResponse> GetOrderByIdAsync(int orderId);

        // ADMIN – VIEW ORDERS
        Task<List<OrderResponse>> GetAllOrdersAsync();

        // ADMIN – UPDATE ORDER STATUS
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        // USER – CANCEL ORDER
        Task<bool> CancelOrderAsync(int orderId, int userId);

        // PAYMENT
        Task<bool> VerifyOrderPaymentAsync(PaymentVerificationRequest request);

        // DIRECT BUY (WITHOUT CART)
        Task<OrderResponse> ProcessDirectBuyAsync(int userId, DirectBuyRequest request);

        // ADMIN – DASHBOARD
        Task<DashBoardResponse> GetDashBoardAsync();

        // RETURNS
        Task<int> CreateReturnRequestAsync(int userId, int orderId, CreateReturnRequest request);
        Task<int> AddReturnRequestAsync(ReturnRequest request);
        Task<List<ReturnResponse>> GetAllReturnRequestsAsync();


    }



    
}
