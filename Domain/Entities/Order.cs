using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class Order :BaseEntity
    {

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string ReceiverName { get; set; } = string.Empty;
       
        public string ShippingAddress { get; set; } = string.Empty;
     
        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;
  
        public string PinNumber { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
            
        public string PaymentMethod { get; set; } = "COD";

        public virtual User user { get; private  set; }

    
        public decimal TotalPrice { get; private set; }

        public DateTime? ShippingDate { get; set; }



        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Order(int userId, string shippingAddress, string paymentMethod, string mobileNumber, decimal totalAmount)
        {
            UserId = userId;
            ShippingAddress = shippingAddress;
            PaymentMethod = paymentMethod;
            MobileNumber = mobileNumber;

            OrderDate = DateTime.UtcNow;
            Status = "Pending";
            OrderItems = new List<OrderItem>();
        }


    }
}
