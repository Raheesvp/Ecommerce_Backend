using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public  class PaymentVerificationRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string TransactionId { get; set; } 
        public string? ProviderOrderId { get; set; } 

        

        [Required]
        public string Status { get; set; } 

        public string? PaymentMethod { get; set; } 
    }
}
