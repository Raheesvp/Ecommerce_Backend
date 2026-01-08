using Domain.Common;
using Org.BouncyCastle.Asn1.Mozilla;
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

        public string ProductName { get;  set; } = null!;

        public int Quantity { get;  set; }
        public decimal Price { get; set; } 

        public decimal UnitPrice { get; set; }

        public string? ImageUrl { get; set; }


     
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public   OrderItem() { }

        public int Id { get; set; }



       

   



        public OrderItem(Product product, int quantity)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            ImageUrl = product.ImageUrl;
            UnitPrice = product.Price * quantity;
            Price = product.Price;
            Quantity = quantity;

          
        }
    }
}
