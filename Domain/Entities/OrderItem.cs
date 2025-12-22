using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; private set; } = null!;


      
        public int Quantity { get; private set; }
        public decimal Price { get;private set; }  // Snapshot of price

     

        //public int Price { get; set; }

        //[ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        private OrderItem() { }

        // 2. The Missing Constructor (Fixes your error!)
        public OrderItem(Product product, int quantity)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Quantity = quantity;

          
        }
    }
}
