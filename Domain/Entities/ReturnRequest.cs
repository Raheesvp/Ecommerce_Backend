using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class ReturnRequest
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        //public string ProductName { get; set; }
        public string UserId { get; set; }
        public string Reason { get; set; } 

        
        public string Description { get; set; }
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

     
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    public enum ReturnStatus { Pending, Approved, Rejected, PickedUp, Refunded }
}

