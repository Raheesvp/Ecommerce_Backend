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

            modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive);

            modelBuilder.Entity<Product>()
        .Property(p => p.Price)
        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
     .HasOne<CategoryEntity>()
     .WithMany()
     .HasForeignKey("CategoryId")
     .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // 3. Resolve Relationship Warning
            // Apply the same query filter to related entities to ensure consistency
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.user.IsDeleted);




        }

    }
}
