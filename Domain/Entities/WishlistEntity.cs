using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WishlistEntity
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int ProductId { get; private set; }

        public Product Product { get; set; } = null!;

        private WishlistEntity() { }

        public WishlistEntity(int userId, int productId)
        {
            UserId = userId;
            ProductId = productId;
         
        }
    }
}
