using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order
{
    public class CreateOrderRequest
    {
       
        [Required(ErrorMessage = "Receiver Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be at least 3 characters")]
        public string ReceiverName { get; set; } = string.Empty;

    
        [Required(ErrorMessage = "Shipping Address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be at least 10 characters long")]
        public string ShippingAddress { get; set; } = string.Empty;

  
        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

       
        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

      
        [Required(ErrorMessage = "Pin Code is required")]
        [RegularExpression(@"^\d{5,10}$", ErrorMessage = "Pin Code must be numbers only (5-10 digits)")]
        public string PinNumber { get; set; } = string.Empty;

      
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 digits")]
        public string MobileNumber { get; set; } = string.Empty;

       
        [Required(ErrorMessage = "Payment Method is required")]
        public string PaymentMethod { get; set; } = "COD";
    }
}