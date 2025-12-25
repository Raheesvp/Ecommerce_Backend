using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Cart
{
    public  class CartItemResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public int Stock { get; set; }
    }
}
