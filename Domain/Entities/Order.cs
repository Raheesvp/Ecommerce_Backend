using Domain.Common;
using Domain.Entities;
using Domain.Enums;

public class Order : BaseEntity
{
    public Order()
    {
        OrderItems = new List<OrderItem>();
    }

    public int UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // --- Added Shipping Details ---
    public string ReceiverName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty; // Added this!
    public string ShippingAddress { get; set; } = string.Empty;

    public DateTime? ShippingDate { get; set; } 
    public string City { get; set; } = string.Empty;        // Added this!
    public string State { get; set; } = string.Empty;       // Added this!
    public string PinNumber { get; set; } = string.Empty;   // Added this!

    public string PaymentMethod { get; set; } = "COD";

    // --- Made these Nullable so COD doesn't crash ---
    public string? PaymentReference { get; set; }
    public DateTime? PaidOn { get; set; }
    public string? RazorPayOrderId { get; set; } // Added '?' for null safety

    public virtual User user { get; private set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }

    // Updated Constructor
    public Order(int userId, string receiverName, string mobileNumber, string shippingAddress, string city, string state, string pinNumber, decimal totalAmount)
    {
        UserId = userId;
        ReceiverName = receiverName;
        MobileNumber = mobileNumber;
        ShippingAddress = shippingAddress;
        City = city;
        State = state;
        PinNumber = pinNumber;
        TotalAmount = totalAmount;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        OrderItems = new List<OrderItem>();
    }
}