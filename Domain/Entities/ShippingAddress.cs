using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class ShippingAddress :BaseEntity
    {
        public int UserId { get; set; } // Foreign Key to User
        public string AddressLine { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public int Pincode { get; set; }
        public long Phone { get; set; }
        public bool IsActive { get; set; } = false; // To mark the default address

        public virtual User User { get; set; } = null!; // Navigation prop
    }
}
