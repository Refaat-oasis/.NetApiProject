using ApiProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ApiProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Data for Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661" },
                new Category { Id = 2, Name = "Fashion", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050" },
                new Category { Id = 3, Name = "Home Decor", ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e35a6" },
                new Category { Id = 4, Name = "Books", ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d" },
                new Category { Id = 5, Name = "Beauty", ImageUrl = "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9" }
            );

            // Configure relationships if needed (EF will mostly handle these automatically based on our models)
            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");
                
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Product>()
    .HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .HasForeignKey(p => p.CategoryId);

            builder.Entity<Product>()
    .HasOne(p => p.Seller)
    .WithMany()
    .HasForeignKey(p => p.SellerId)
    .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<Review>()
.HasIndex(r => new { r.UserId, r.ProductId })
.IsUnique();
        }
    }
}
