
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

        //public DbSet<Review> Reviews { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<ReviewImage> ReviewImages { get; set; }

        //database table for return request

        public DbSet<ReturnRequest> ReturnRequests { get; set; }






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


            modelBuilder.Entity<ReviewImage>()
                 .HasOne(ri => ri.Review)
                 .WithMany(r => r.ReviewImages)
                 .HasForeignKey(ri => ri.ReviewId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReturnRequest>()
                .HasIndex(r => new { r.OrderId, r.ProductId }).IsUnique();


            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.user.IsDeleted);




        }

    }
}
