using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Order;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
        }

        public async Task<int> PlaceOrderAsync(int userId, CreateOrderRequest request)
        {
            // 1. Get User's Cart
            var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cartItems == null || !cartItems.Any())
            {
                throw new InvalidOperationException("Cannot place order. Cart is empty.");
            }

            // 2. Calculate Total
            decimal totalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);

            // 3. Create the Order Object
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = totalAmount,
                ShippingAddress = request.ShippingAddress,
                PaymentMethod = request.PaymentMethod,
                MobileNumber = request.MobileNumber,
                OrderItems = new List<OrderItem>()
            };

            // 4. Create Order Items (Snapshot of data)
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem(cartItem.Product, cartItem.Quantity);

                order.OrderItems.Add(orderItem);
            }

            // 5. Save Order to Database
            await _orderRepository.CreateAsync(order);

            // 6. CLEAR THE CART (Important!)
            await _cartRepository.ClearCartAsync(userId);

            return order.Id; // Return Order ID so frontend can show confirmation
        }

        public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
        {

            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            // Map Entity->DTO manually(or use AutoMapper if you have it)
            var response = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PaymentMethod = o.PaymentMethod,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,

                    Quantity = i.Quantity,
                    Price = i.Price,
                    ImageUrl = i.Product != null ? i.Product.ImageUrl : ""
                }).ToList()
            }).ToList();

            return response;
        }
    

    //get order by specific Id

    public async Task<OrderResponse> GetOrderByIdAsync(int userId, int orderId)
        {
            // 1. Fetch Order
            var order = await _orderRepository.GetByIdAsync(orderId);

            // 2. Check if Order exists
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            // 3. SECURITY CHECK: Verify Ownership
            // Prevents User A from seeing User B's order by guessing the ID
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to view this order.");

            // 4. Map to DTO (Same logic as GetUserOrdersAsync)
            return new OrderResponse
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress, // Or ShippingAddress if you fixed the typo
                PaymentMethod = order.PaymentMethod,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "Product Not Found",
                    Price = i.Product != null ? i.Product.Price : 0,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product != null ? i.Product.ImageUrl : ""
                }).ToList()
            };
        }
    }
}


    
