using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        // 1. FIX: Added IUnitOfWork to the constructor
        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork; // Now it won't be null!
        }

        public async Task<int> PlaceOrderAsync(int userId, CreateOrderRequest request)
        {
            // This transaction ensures if ANY step fails, everything is undone.

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);

                if (cartItems == null || !cartItems.Any())
                {
                    throw new InvalidOperationException("Cannot place order. Cart is empty.");
                }

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null) throw new Exception("Product Not Found");

                    // Check Stock
                    if (product.Stock < cartItem.Quantity)
                    {
                        throw new InvalidOperationException($"Product '{product.Name}' is out of stock.");
                    }

                    // Reduce Stock
                    product.Stock -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // 2. FIX: Create "Snapshot" instead of passing the full Product entity
                    // This prevents the "500 Internal Server Error" / "Identity Insert" crash
                    var orderItem = new OrderItem(product, cartItem.Quantity);
                 

                    orderItems.Add(orderItem);
                    totalAmount += (product.Price * cartItem.Quantity);
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = totalAmount,
                    ShippingAddress = request.ShippingAddress,
                    PaymentMethod = request.PaymentMethod,
                    MobileNumber = request.MobileNumber,
                    OrderItems = orderItems
                };

                await _orderRepository.CreateAsync(order);
                await _cartRepository.ClearCartAsync(userId);

                await _unitOfWork.CommitTransactionAsync();

                return order.Id;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return false;

                // Restock if Cancelled
                if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled.ToString())
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            await _productRepository.UpdateAsync(product);
                        }
                    }
                }

                order.Status = newStatus.ToString();
                if (newStatus == OrderStatus.Shipped)
                {
                    order.ShippingDate = DateTime.UtcNow;
                }

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // 3. FIX: Simplified methods using the helper below
        public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponse> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");
            return MapToDto(order);
        }

        public async Task<List<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        // Helper Method to keep code clean (DRY Principle)
        private static OrderResponse MapToDto(Order o)
        {
            return new OrderResponse
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PaymentMethod = o.PaymentMethod,
                UserEmail = o.user?.Email, // Assuming 'user' navigation property exists
                OrderItems = o.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown",
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product?.Images?.FirstOrDefault()?.Url
                }).ToList()
            };
        }
    }
}