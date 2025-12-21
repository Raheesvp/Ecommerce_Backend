namespace Domain.Entities
{
    public class CartEntity
    {
        public int Id { get; private set; }

        public int UserId { get; private set; }
        public int ProductId { get; private set; }

        public int Quantity { get; private set; }

        public Product Product { get; private set; } = null!;

        private CartEntity() { }

        public CartEntity(int userId, int productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }

        //adding to the cart
        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            Quantity = quantity;
        }

        //increase the quantity of the product

        public void IncreaseQuantity(int amount = 1)
        {
            UpdateQuantity(Quantity + amount);
        }

        //delete specific product from the cart 

       
    }
}
