using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public class ReturnResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } 
        public DateTime RequestedAt { get; set; }
    }
}
