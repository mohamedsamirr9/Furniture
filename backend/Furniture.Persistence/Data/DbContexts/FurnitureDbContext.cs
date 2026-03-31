using Furniture.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Persistence.Data.DbContexts
{
    public class FurnitureDbContext: DbContext
    {
        public FurnitureDbContext(DbContextOptions options): base (options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(c => c.Description)
                      .HasMaxLength(500);

                entity.Property(c => c.Image)
                      .HasMaxLength(200);
            });

            //product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(300);

                entity.Property(p => p.Description)
                      .HasMaxLength(2000);

               

                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Product → Category  (Many-to-One)
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Product → User (Many-to-One)
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //product image 
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(pi => pi.Id);

                entity.Property(pi => pi.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(500);

                // ProductImage → Product  (Many-to-One)
                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //application user 
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(300);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.Role)
                      .HasMaxLength(50);


                entity.Property(u => u.Address)
                      .HasMaxLength(500);

                entity.ToTable(Tb =>
                {
                    Tb.HasCheckConstraint("EmailCheck", "Email Like '_%@_%._%'");
                    Tb.HasCheckConstraint("PhoneCheck", "PhoneNumber LIKE '01%' AND PhoneNumber NOT LIKE '%[^0-9]%'");
                });

                entity.Property(u => u.RegisterdAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(u => u.IsDeleted)
                      .HasDefaultValue(false);
            });

            //cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.Id);


                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(c => c.UserId).IsUnique();

                // Cart → User (One-to-One)
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Cart)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

             
            });

            //cart item
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(c => new { c.ProductId, c.CartId });

                entity.Property(ci => ci.Quantity)
                      .IsRequired();

                entity.Property(ci => ci.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                // CartItem → Cart  (Many-to-One)
                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CartItem → Product  (Many-to-One)
                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            //order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Status)
                      .HasMaxLength(50);

                entity.Property(o => o.ShippingAddress)
                      .HasMaxLength(500);

                entity.Property(o => o.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Order →  User (Many-to-One)
                entity.HasOne(oi => oi.User)
                      .WithMany(o => o.Orders)
                      .HasForeignKey(oi => oi.UserId)
                      .OnDelete(DeleteBehavior.Cascade);



            });

            //order item
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => new { oi.OrderId, oi.ProductId });

                entity.Property(oi => oi.Quantity)
                      .IsRequired();

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                // OrderItem → Order  (Many-to-One)
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // OrderItem → Product  (Many-to-One)
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

          

            //offer
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(o => o.DeliveryDays)
                      .IsRequired();

                entity.Property(o => o.IsAccepted)
                      .HasDefaultValue(false);

              

                // Offer → User (Many-to-One)
                entity.HasOne(o => o.Seller)
                      .WithMany(u => u.Offers)
                      .HasForeignKey(o => o.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);


            });

            //favourite
            modelBuilder.Entity<Favourite>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasIndex(f => new { f.UserId, f.ProductId })
                      .IsUnique();

                // Favourite → User  (Many-to-One) 
                entity.HasOne(f => f.User)
                      .WithMany(u => u.Favourites)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Favourite → Product  (Many-to-One) 
                entity.HasOne(f => f.Product)
                      .WithMany(p => p.Favourites)
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Rating)
                      .IsRequired();

                entity.Property(r => r.Message)
                      .HasMaxLength(2000);

                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Review → User  (Many-to-One)  
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Review → Product  (Many-to-One)
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //complaint
            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Description)
                      .HasMaxLength(2000);

                entity.Property(c => c.Status)
                      .HasMaxLength(50);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Complaint → User  (Many-to-One) 
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Complaints)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);


            });

            //payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.PaymentMethod)
                      .HasMaxLength(100);

                entity.Property(p => p.PaymentStatus)
                      .HasMaxLength(50);

                entity.Property(p => p.TransactionRef)
                      .HasMaxLength(300);

                entity.Property(p => p.PaymentDate)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Payment → Order  (One-to-One)  
                entity.HasOne(p => p.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Payment> Payments { get; set; }


    }
}
