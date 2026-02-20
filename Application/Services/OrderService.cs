using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs.Admin;
using Application.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
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
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

       public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,IUserService userService,
            INotificationService  notificationService

            
            )
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
          
            _notificationService = notificationService;
        }

        public async Task<int> PlaceOrderAsync(int userId, CreateOrderRequest request)
        {
            int orderId = 0;

            await _unitOfWork.ExecuteAsync(async () =>
            {
                var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cartItems == null || !cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null)
                        throw new Exception($"Product with ID {cartItem.ProductId} Not Found");

                    if (product.Stock < cartItem.Quantity)
                        throw new InvalidOperationException($"Product '{product.Name}' is out of stock.");

                    var orderItem = new OrderItem(product, cartItem.Quantity);
                    orderItems.Add(orderItem);
                    totalAmount += product.Price * cartItem.Quantity;
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
                orderId = order.Id;
            });

            await _notificationService.OrderPlacedAsync("New Order Placed", orderId);
            return orderId;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            await _unitOfWork.ExecuteAsync(async () =>
            {
                OrderStatus currentStatus = order.Status;

                if (currentStatus == OrderStatus.Cancelled || currentStatus == OrderStatus.Delivered)
                    throw new InvalidOperationException($"Order is already {currentStatus}.");

                if (newStatus == OrderStatus.Cancelled)
                {
                    if (currentStatus >= OrderStatus.Shipped)
                        throw new InvalidOperationException("Cannot cancel shipped order.");

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
                else
                {
                    if ((int)newStatus != (int)currentStatus + 1)
                        throw new InvalidOperationException("Invalid status transition.");
                }

                order.Status = newStatus;

                if (newStatus == OrderStatus.Shipped)
                    order.ShippingDate = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
            });

            return true;
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
                Status = ((int)o.Status).ToString(),
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
            await _unitOfWork.ExecuteAsync(async () =>
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) throw new KeyNotFoundException("Order not found");
                if (order.UserId != userId) throw new UnauthorizedAccessException();

                if (order.Status != OrderStatus.Pending &&
                    order.Status != OrderStatus.Processing)
                    throw new InvalidOperationException("Cannot cancel.");

                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);
            });

            return true;
        }


        public async Task<bool> VerifyOrderPaymentAsync(PaymentVerificationRequest request)
        {
            bool success = false;

            await _unitOfWork.ExecuteAsync(async () =>
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null) return;

                if (!string.Equals(request.Status, "success", StringComparison.OrdinalIgnoreCase))
                    return;

                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException("Insufficient stock.");

                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                order.Status = OrderStatus.Processing;
                order.PaymentReference = request.TransactionId;
                order.PaidOn = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                await _cartRepository.ClearCartAsync(order.UserId);

                success = true;
            });

            if (success)
                await _notificationService.OrderPlacedAsync("Payment Successful", request.OrderId);

            return success;
        }




        public async Task<OrderResponse> ProcessDirectBuyAsync(int userId, DirectBuyRequest request)
        {
            Order order = null!;

            await _unitOfWork.ExecuteAsync(async () =>
            {
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product.Stock < request.Quantity)
                    throw new InvalidOperationException("Stock insufficient.");

                decimal totalAmount = product.Price * request.Quantity;

                order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    PaymentMethod = "Online",
                    OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price,
                    Price = totalAmount
                }
            }
                };

                await _orderRepository.CreateAsync(order);
            });

            return MapToDto(order);
        }


        //admin dashbord service 

        public async Task<DashBoardResponse> GetDashBoardAsync()
        {
            var today = DateTime.UtcNow.Date;

            
            var orders = await _orderRepository.GetAllAsync(); 
            var users = await _userService.GetAllUsersAsync();
            var products = await _productRepository.GetAllAsync();


            var validOrders = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();
            var todayOrdersList = validOrders.Where(o => o.OrderDate.Date == today).ToList();

          
            var salesHistory = validOrders
                .Where(o => o.OrderDate >= today.AddDays(-7))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesChartDTO
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            // 4. Calculate Top Selling Products
            var topSelling = validOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new RecentStockDTO
                {
                    ProductName = g.Key,
                    Value = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Value)
                .Take(5).ToList();

            // 5. Calculate Low Stock Products
            var lowStock = products
                .Where(p => p.IsActive && p.Stock < 10)
                .OrderBy(p => p.Stock)
                .Take(5)
                .Select(p => new ProductStockDTO
                {
                    ProductName = p.Name,
                    Value = p.Stock
                }).ToList();

            return new DashBoardResponse
            {
                TotalRevenue = validOrders.Sum(o => o.TotalAmount),
                TodayRevenue = todayOrdersList.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                TodayOrders = todayOrdersList.Count,
                TotalUsers = users.Count(u => u.Role == Roles.User.ToString()),
                TotalProducts = products.Count,
                SalesHistory = salesHistory,
                TopSellingProducts = topSelling,
                LowStockProducts = lowStock,
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).Select(o => new RecentOrderDTO
                {
                    OrderId = o.Id,
                    CustomerName = o.user?.FullName ?? "Unknown",
                    Amount = o.TotalAmount,
                    Status = o.Status.ToString()
                }).ToList()
            };
        }

        public async Task<int> CreateReturnRequestAsync(int userId, int orderId, CreateReturnRequest request)
        {
            
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
                throw new Exception("Order not found.");

            
            if (order.Status != OrderStatus.Delivered)
                throw new Exception("You can only return items that have been delivered.");

           
            var returnRequest = new ReturnRequest
            {
                OrderId = order.Id,
                ProductId = request.ProductId,
                UserId = userId.ToString(),
                Reason = request.Reason,
                Description = request.Description,
                Status = ReturnStatus.Pending
            };

        
            return await _orderRepository.AddReturnRequestAsync(returnRequest);
        }

        public async Task<int> AddReturnRequestAsync(ReturnRequest returnRequest)
        {
           
            return await _orderRepository.AddReturnRequestAsync(returnRequest);
        }

        public async Task<List<ReturnResponse>> GetAllReturnRequestsAsync()
        {
           
            var returns = await _orderRepository.GetAllReturnRequestsAsync();

            return returns.Select(r => new ReturnResponse
            {
                Id = r.Id,
                OrderId = r.OrderId,
                ProductName = r.Product?.Name ?? "Unknown Product",
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt
            }).ToList();
        }
    }
    }
 
