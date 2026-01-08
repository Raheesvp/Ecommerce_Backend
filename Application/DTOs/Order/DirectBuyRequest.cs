using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public class DirectBuyRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ReceiverName { get; set; }
        public string MobileNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }  // Add this
        public string State { get; set; } // Add this
        public string PinNumber { get; set; } // Add this
                                              
    }
}
