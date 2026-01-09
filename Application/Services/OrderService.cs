using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Admin;
using Application.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Razor;
using Razorpay.Api;
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

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> PlaceOrderAsync(int userId, CreateOrderRequest request)
        {
            // Start transaction to ensure Order and OrderItems are created atomically
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cartItems == null || !cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null) throw new Exception($"Product with ID {cartItem.ProductId} Not Found");

                    // Validate stock availability
                    if (product.Stock < cartItem.Quantity)
                        throw new InvalidOperationException($"Product '{product.Name}' is out of stock.");

                    var orderItem = new OrderItem(product, cartItem.Quantity);
                    orderItems.Add(orderItem);
                    totalAmount += (product.Price * cartItem.Quantity);
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    ReceiverName = request.ReceiverName,
                    MobileNumber = request.MobileNumber,
                    ShippingAddress = request.ShippingAddress,
                    City = request.City,
                    State = request.State,
                    PinNumber = request.PinNumber,
                    PaymentMethod = request.PaymentMethod,
                    OrderItems = orderItems
                };

                await _orderRepository.CreateAsync(order);

                // Commit creates the 'Pending' order in the database
                await _unitOfWork.CommitTransactionAsync();

                return order.Id;
            }
            catch (Exception)
            {
                // If anything fails, undo the order creation
                await _unitOfWork.RollbackTransactionAsync();
                throw; // Re-throw to let the Controller handle the error message
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return false;

                OrderStatus currentStatus = order.Status;

                // 1. If already Cancelled or Delivered, no more changes allowed
                if (currentStatus == OrderStatus.Cancelled || currentStatus == OrderStatus.Delivered)
                {
                    throw new InvalidOperationException($"Order is already {currentStatus}. No further updates allowed.");
                }

                // 2. Logic for CANCELLATION
                if (newStatus == OrderStatus.Cancelled)
                {
                    // REQUIREMENT: Cannot cancel if already Shipped
                    if (currentStatus >= OrderStatus.Shipped)
                    {
                        throw new InvalidOperationException("Cannot cancel an order that has already been shipped.");
                    }

                    // Restore Stock
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
                // 3. Logic for STEP-BY-STEP movement (Non-cancel updates)
                else
                {
                    if ((int)newStatus != (int)currentStatus + 1)
                    {
                        throw new InvalidOperationException($"Step-by-step error: Move from {currentStatus} to {(OrderStatus)((int)currentStatus + 1)}.");
                    }
                }

                // Apply changes
                order.Status = newStatus;

                if (newStatus == OrderStatus.Shipped)
                    order.ShippingDate = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync(); // THIS saves to DB
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponse> GetOrderByIdAsync( int orderId)
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

        private static OrderResponse MapToDto(Order o)
        {
            return new OrderResponse
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                // FIX: Convert Enum to String for DTO
                Status = o.Status.ToString(),
                ReceiverName = o.ReceiverName,
                ShippingAddress = o.ShippingAddress,
              
                PaymentMethod = "Online",
                UserEmail = o.user?.Email,
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

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) throw new KeyNotFoundException("Order not found");
                if (order.UserId != userId) throw new UnauthorizedAccessException("Not authorized.");

                // FIX: order.Status is already Enum
                OrderStatus currentStatus = order.Status;

                if (currentStatus != OrderStatus.Pending && currentStatus != OrderStatus.Processing)
                {
                    throw new InvalidOperationException("Order is already Shipped or Delivered and cannot be cancelled.");
                }

                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                // FIX: Assigned Enum directly
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> VerifyOrderPaymentAsync(PaymentVerificationRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Get the order with its items
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null) return false;

                // 2. Only process if payment was successful
                if (request.Status != null && request.Status.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    // 3. REDUCE STOCK
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product == null) throw new Exception($"Product {item.ProductId} no longer exists.");

                        if (product.Stock < item.Quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for {product.Name}. Please contact support.");
                        }

                        product.Stock -= item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }

                    // 4. Update Order Details
                    order.Status = OrderStatus.Processing;
                    order.PaymentReference = request.TransactionId;
                    order.PaidOn = DateTime.UtcNow;

                    await _orderRepository.UpdateAsync(order);

                    // 5. CRITICAL: Clear the user's cart
                    // This ensures the items disappear from the UI cart after payment
                    await _cartRepository.ClearCartAsync(order.UserId);

                    // 6. COMMIT ALL CHANGES
                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }

                // If payment failed at the gateway, we do nothing. 
                // Items stay in the cart so user can try again.
                return false;
            }
            catch (Exception ex)
            {
                // If anything fails (Database error, Cart error, Stock error), UNDO EVERYTHING
                await _unitOfWork.RollbackTransactionAsync();

                // Log the error for debugging
                Console.WriteLine($"Payment Verification Error: {ex.Message}");
                throw;
            }
        }



        public async Task<OrderResponse> ProcessDirectBuyAsync(int userId, DirectBuyRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null) throw new KeyNotFoundException("Product not found.");
                if (product.Stock < request.Quantity) throw new InvalidOperationException("Stock insufficient.");

                decimal totalAmount = product.Price * request.Quantity;

                var client = new Razorpay.Api.RazorpayClient("rzp_test_S0goOJJ0kMzST1", "n6vk98LSC8BnvHAwz6yPrPUz");
                Dictionary<string, object> options = new Dictionary<string, object>
        {
            { "amount", (int)(totalAmount * 100) },
            { "currency", "INR" },
            { "receipt", Guid.NewGuid().ToString() }
        };

                Razorpay.Api.Order razorpayOrder = client.Order.Create(options);

                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    RazorPayOrderId = razorpayOrder["id"]?.ToString() ?? "PENDING_ID",

                    // USE FALLBACKS TO PREVENT NULL ERROR 515
                    ReceiverName = request.ReceiverName,
                    MobileNumber = request.MobileNumber ,
                    ShippingAddress = request.ShippingAddress ,
                    City = request.City  ,
                    State = request.State ,
                    PinNumber = request.PinNumber,
                    PaymentMethod = "Online",

                    OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price, // Fixed
                    Price = totalAmount        // Fixed
                }
            }
                };

                await _orderRepository.CreateAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(order);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                // This will now show the actual inner message in your logs
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Direct Buy Failed: {innerMsg}");
            }
        }

        //admin dashbord service 

        public async Task<DashBoardResponse> GetDashBoardAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return new DashBoardResponse
            {
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                TotalUsers = users.Count(u => u.Role == Roles.User),
                TotalProducts = products.Count,
                RecentOrders = orders
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new RecentOrderDTO
            {
                OrderId = o.Id,
                CustomerName = o.user?.FullName ?? "Unknown",
                Amount = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList() // No 'Async' here because 'orders' is already a List
            };
        }
        }

    }
