using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public  class CreateOrderRequest
    {
        [Required(ErrorMessage = "Shipping Address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be at least 10 characters long")]
        public string ShippingAddress { get; set; }
        [Required(ErrorMessage ="Payment Method is required")]
        public string PaymentMethod { get; set; } = "COD";

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 digits")]
        public string MobileNumber { get; set; } = string.Empty;
    
    }
}
