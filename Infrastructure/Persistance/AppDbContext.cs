using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        //create table of users

        public DbSet<User> Users { get; set; }

        //create table of products
        public DbSet<Product> Products { get; set; }

        //create table wishlist 

        public DbSet<WishlistEntity> Wishlists { get; set; }

        //create table for the cart

        public DbSet<CartEntity> Carts { get; set; }

        //create table for the Order 

        public DbSet<Order> Orders { get; set; }

        //cratea table for category 

        public DbSet<CategoryEntity> CategoryEntities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WishlistEntity>()
                .HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();

            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);


            
        }

    }
}
