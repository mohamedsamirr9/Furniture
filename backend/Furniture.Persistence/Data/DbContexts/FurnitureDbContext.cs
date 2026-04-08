using Furniture.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Persistence.Data.DbContexts
{
    public class FurnitureDbContext: IdentityDbContext<ApplicationUser>
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

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

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

                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //application user 
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(300);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

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

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Status);

                entity.Property(o => o.ShippingAddress)
                      .HasMaxLength(500);

                entity.Property(o => o.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Order → ShippingRequest (1-1)
                entity.HasOne(o => o.ShippingRequest)
                      .WithOne(sr => sr.Order)
                      .HasForeignKey<ShippingRequest>(sr => sr.OrderId);

                //  Order → Delivery (1-1)
                entity.HasOne(o => o.Delivery)
                      .WithOne(d => d.Order)
                      .HasForeignKey<Delivery>(d => d.OrderId);
            });

            //order item
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => new { oi.OrderId, oi.ProductId });

                entity.Property(oi => oi.Quantity)
                      .IsRequired();

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(oi => oi.Seller)
                      .WithMany()
                      .HasForeignKey(oi => oi.SellerId)
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

                entity.HasOne(o => o.Seller)
                      .WithMany(u => u.Offers)
                      .HasForeignKey(o => o.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Offer → CustomRequest
                entity.HasOne(o => o.CustomRequest)
                      .WithMany(cr => cr.Offers)
                      .HasForeignKey(o => o.CustomRequestId);

                //  Offer → Order 
                entity.HasOne(o => o.Order)
                      .WithOne()
                      .HasForeignKey<Offer>(o => o.OrderId)
                      .IsRequired(false);
            });

            //custom request
            modelBuilder.Entity<CustomRequest>(entity =>
            {
                entity.HasKey(cr => cr.Id);
                entity.Property(cr => cr.Budget)
                        .HasColumnType("decimal(18,2)");

                // CustomRequest → Buyer
                entity.HasOne(cr => cr.Buyer)
                      .WithMany(u => u.CustomRequests)
                      .HasForeignKey(cr => cr.BuyerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //shipping request
            modelBuilder.Entity<ShippingRequest>(entity =>
            {
                entity.HasKey(sr => sr.Id);

                entity.HasMany(sr => sr.Bids)
                      .WithOne(b => b.ShippingRequest)
                      .HasForeignKey(b => b.ShippingRequestId);
            });

            //shipping bid
            modelBuilder.Entity<ShippingBid>(entity =>
            {
                entity.HasKey(sb => sb.Id);
                entity.Property(sb => sb.Price)
                 .HasColumnType("decimal(18,2)");

                //  ShippingBid → Shipper
                entity.HasOne(sb => sb.Shipper)
               .WithMany()
               .HasForeignKey(sb => sb.ShipperId)
               .OnDelete(DeleteBehavior.Restrict);
            });

            //delivery
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasKey(d => d.Id);

                //  Delivery → Shipper
                entity.HasOne(sb => sb.Shipper)
               .WithMany()
               .HasForeignKey(sb => sb.ShipperId)
               .OnDelete(DeleteBehavior.Restrict);
            });

            //favourite
            modelBuilder.Entity<Favourite>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasIndex(f => new { f.UserId, f.ProductId })
                      .IsUnique();

                entity.HasOne(f => f.User)
                      .WithMany(u => u.Favourites)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

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

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

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

                entity.Property(c => c.Status);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Complaints)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                //  Complaint → Order
                entity.HasOne(c => c.Order)
                      .WithMany()
                      .HasForeignKey(c => c.OrderId);
            });

            //payment
            modelBuilder.Entity<Payment>(entity =>
            {
                  entity.HasKey(p => p.Id);

                  entity.Property(p => p.Amount)
                        .HasColumnType("decimal(18,2)");

                  // Payment → Order (1-1) 
                  entity.HasOne(p => p.Order)
                        .WithOne(o => o.Payment)
                        .HasForeignKey<Payment>(p => p.OrderId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
            
            //seller profile
            modelBuilder.Entity<SellerProfile>(entity =>
            {
                  entity.HasKey(s => s.Id);

                  entity.Property(s => s.StoreName)
                        .IsRequired()
                        .HasMaxLength(200);

                  entity.Property(s => s.StoreDescription)
                        .HasMaxLength(1000);

                  entity.Property(s => s.CommissionRate)
                        .HasColumnType("decimal(5,2)");

                  entity.Property(s => s.CreatedAt)
                        .HasDefaultValueSql("GETUTCDATE()");

                  entity.HasOne(s => s.User)
                        .WithOne(u => u.SellerProfile)
                        .HasForeignKey<SellerProfile>(s => s.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                  entity.HasIndex(s => s.UserId).IsUnique();
            });

            //seller payout
            modelBuilder.Entity<SellerPayout>(entity =>
            {
                  entity.HasKey(p => p.Id);

                  entity.Property(p => p.OrderItemsTotal)
                        .HasColumnType("decimal(18,2)");

                  entity.Property(p => p.CommissionAmount)
                        .HasColumnType("decimal(18,2)");

                  entity.Property(p => p.NetAmount)
                        .HasColumnType("decimal(18,2)");

                  entity.Property(p => p.CreatedAt)
                        .HasDefaultValueSql("GETUTCDATE()");

                  entity.HasOne(p => p.SellerProfile)
                        .WithMany(s => s.Payouts)
                        .HasForeignKey(p => p.SellerProfileId)
                        .OnDelete(DeleteBehavior.Restrict);

                  entity.HasOne(p => p.Order)
                        .WithMany(o => o.SellerPayouts)
                        .HasForeignKey(p => p.OrderId)
                        .OnDelete(DeleteBehavior.Restrict);
            });
        }
        //public DbSet<ApplicationUser> ApplicationUsers { get; set; }
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

        public DbSet<ShippingRequest> ShippingRequests { get; set; }
        public DbSet<ShippingBid> ShippingBids { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<CustomRequest> CustomRequests { get; set; }

        public DbSet<SellerProfile> SellerProfiles { get; set; } = null!;
        public DbSet<SellerPayout> SellerPayouts { get; set; } = null!;


    }
}
