using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class Order :BaseEntity
    {
        
        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string ShippingAddress { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
            

        public string PaymentMethod { get; set; } = "COD";

 

        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }
 
       

    }
}
