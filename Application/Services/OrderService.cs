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

                
                    if (product.Stock < cartItem.Quantity)
                    {
                        throw new InvalidOperationException($"Product '{product.Name}' is out of stock.");
                    }

                    
                    //product.Stock -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);

              
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

        public async Task<bool> CancelOrderAsync(int orderId,int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);

                if (order == null)
                    throw new KeyNotFoundException("Order not found");

              
                if (order.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to cancel this order.");

               
                if (order.Status == "Shipped" || order.Status == "Delivered" || order.Status == "Cancelled")
                    throw new InvalidOperationException("This order cannot be cancelled.");

   
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity; 
                        await _productRepository.UpdateAsync(product);
                    }
                }

            
                order.Status = "Cancelled"; 

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
        }

     
    }
