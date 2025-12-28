using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public decimal OriginalPrice { get; private set; }
        public int Stock { get;  set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

      
        public string ImageUrl { get; private set; } = string.Empty;

        public virtual ICollection<ProductImage> Images { get; private set; }

        public string Offer { get; private set; }
        public int Rating { get; private set; }
        public bool Featured { get; private set; }

        public bool IsActive { get; set; } = true;

        private Product() { } // Required for EF Core

        // Updated Constructor to force ImageUrl when creating a product
        public Product(
            string name,
            decimal price,
            decimal originalPrice,
            int stock,
            string category,
            string description,
            string offer,
            int rating,
            bool featured,
            string imageUrl) // <--- ADDED THIS PARAMETER
        {
            Validate(name, price, stock, category, rating);

            Name = name.Trim();
            Price = price;
            OriginalPrice = originalPrice;
            Stock = stock;
            Category = category.Trim();
            Description = description;
            Images = new List<ProductImage>();
            Offer = offer;
            Rating = rating;
            Featured = featured;

            // --- Set the Main Image ---
            ImageUrl = imageUrl;
        }

        public void AddImage(string url)
        {
            if (Images == null) Images = new List<ProductImage>();

            Images.Add(new ProductImage { Url = url });

            // Auto-set the main image if it's currently empty
            if (string.IsNullOrEmpty(ImageUrl))
            {
                ImageUrl = url;
            }
        }

        public void Update(
            string? name,
            decimal? price,
            int? stock,
            string? category,
            string? description,
            string? imageUrl, // <--- Added ability to update main image
            int? offer,
            bool? featured)
        {
            if (name != null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be empty");
                Name = name.Trim();
            }

            if (price.HasValue)
            {
                if (price <= 0)
                    throw new ArgumentException("Price must be positive");
                Price = price.Value;
            }

            if (stock.HasValue)
            {
                if (stock < 0)
                    throw new ArgumentException("Stock cannot be negative");
                Stock = stock.Value;
            }

            if (category != null) Category = category.Trim();
            if (description != null) Description = description;

            // --- Update Main Image ---
            if (imageUrl != null) ImageUrl = imageUrl;

            if (offer.HasValue && offer < 0) throw new ArgumentException("Offer cannot be negative");

            if (featured.HasValue) Featured = featured.Value;
        }

        private static void Validate(string name, decimal price, int stock, string category, int rating)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required");
            if (price <= 0) throw new ArgumentException("Price must be positive");
            if (stock < 0) throw new ArgumentException("Stock cannot be negative");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category is required");
            if (rating < 0 || rating > 5) throw new ArgumentException("Rating must be between 0 and 5");
        }
    }
}