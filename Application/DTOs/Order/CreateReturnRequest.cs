using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public class CreateReturnRequest
    {
        public int ProductId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }
}
