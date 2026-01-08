using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin
{
    public  class DashBoardResponse
    {
        public decimal TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalUsers { get; set; }

        public int TotalProducts { get; set; }

        public List<RecentOrderDTO> RecentOrders { get; set; } = new();


    }

    public class RecentOrderDTO
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; }
        
        public decimal Amount { get; set; }

        public string Status { get; set; }
    }
}
