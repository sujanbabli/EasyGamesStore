using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EasyGamesStore.Models;

namespace EasyGamesStore.Data
{
    // The ApplicationDbContext serves as the main bridge between the application and the database.
    // It extends IdentityDbContext, which already manages user authentication and authorization tables (Users, Roles, Claims, etc.).
    // In addition to identity-related entities, this class defines all custom tables used in the EasyGamesStore application.
    public class ApplicationDbContext : IdentityDbContext
    {
        // Constructor that receives configuration options and passes them to the base DbContext.
        // This allows Entity Framework Core to connect to the correct database based on settings defined in appsettings.json.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ---------------- Existing Tables ----------------
        // Table that stores historical sales data for each user.
        public DbSet<UserSalesHistory> UserSalesHistories { get; set; }

        // Table that stores all product information available in the owner’s main inventory.
        public DbSet<StockItem> StockItems { get; set; }

        // Tables used for online customer orders (not related to the shop POS system).
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // ---------------- New Tables for Assignment 3 ----------------
        // Each of the following tables represents the structure for the new multi-shop system implemented in the assignment.

        // Represents a physical or virtual shop managed by a proprietor.
        public DbSet<Shop> Shops { get; set; }

        // Represents the inventory (stock items) available within each shop.
        public DbSet<ShopStock> ShopStocks { get; set; }

        // Records every stock transfer that occurs between the owner’s main inventory and the shops.
        public DbSet<ShopTransfer> ShopTransfers { get; set; }

        // Represents customer orders that occur at the shop level (using the POS system).
        public DbSet<ShopOrder> ShopOrders { get; set; }

        // Represents the individual items within each ShopOrder.
        public DbSet<ShopOrderItem> ShopOrderItems { get; set; }

        // ---------------- New Tables for Messaging System ----------------
        // These tables support the internal messaging system between the Owner and Users.
        public DbSet<AppMessage> AppMessages { get; set; }
        public DbSet<AppMessageRead> AppMessageReads { get; set; }

        // ---------------- Additional Table for EPOS (Customer Tracking) ----------------
        // Tracks a customer’s purchase history and tier progression over time.
        public DbSet<CustomerStatusHistory> CustomerStatusHistories => Set<CustomerStatusHistory>();


        // ---------------- Model Configuration ----------------
        // The OnModelCreating method is used to define relationships, constraints, and database behavior rules.
        // This ensures consistency, data integrity, and efficient querying in the database.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Always call the base method first so that Identity’s default configurations are applied.
            base.OnModelCreating(builder);

            // ---------- SHOPSTOCK RELATIONSHIPS ----------

            // Enforces that each ShopStock record must be unique for a given combination of Shop and StockItem.
            // This prevents duplication of the same product entry under one shop.
            builder.Entity<ShopStock>()
                .HasIndex(ss => new { ss.ShopId, ss.StockItemId })
                .IsUnique();

            // Defines that if a Shop is deleted, all its related ShopStock entries should also be deleted automatically.
            // This ensures that no orphaned stock records remain.
            builder.Entity<ShopStock>()
                .HasOne(ss => ss.Shop)
                .WithMany()
                .HasForeignKey(ss => ss.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Defines that if a StockItem is deleted from the owner’s inventory,
            // all related ShopStock entries referencing that StockItem are also deleted.
            builder.Entity<ShopStock>()
                .HasOne(ss => ss.StockItem)
                .WithMany()
                .HasForeignKey(ss => ss.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- SHOPORDERITEM RELATIONSHIPS ----------

            // Specifies that when a ShopOrder is deleted, all of its ShopOrderItems should also be deleted.
            // This maintains referential integrity and ensures no orphaned order items exist.
            builder.Entity<ShopOrderItem>()
                .HasOne(soi => soi.ShopOrder)
                .WithMany(o => o.Items)
                .HasForeignKey(soi => soi.ShopOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Specifies that when a StockItem is deleted, any associated ShopOrderItems that reference it are also deleted.
            builder.Entity<ShopOrderItem>()
                .HasOne(soi => soi.StockItem)
                .WithMany()
                .HasForeignKey(soi => soi.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- APP MESSAGING SYSTEM RELATIONSHIPS ----------

            // Ensures that each combination of AppMessage and UserId in AppMessageRead is unique.
            // This prevents multiple read receipts for the same message and user.
            builder.Entity<AppMessageRead>()
                .HasIndex(x => new { x.AppMessageId, x.UserId })
                .IsUnique();

            // Configures a one-to-many relationship:
            // Each AppMessage can have multiple AppMessageRead records, representing different users who received or read it.
            builder.Entity<AppMessageRead>()
                .HasOne(x => x.AppMessage)
                .WithMany()
                .HasForeignKey(x => x.AppMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reinforces the one-to-many relationship between AppMessage and its recipients.
            // When an AppMessage is deleted, all its corresponding read records are also deleted.
            builder.Entity<AppMessageRead>()
                .HasOne(r => r.AppMessage)
                .WithMany(m => m.Recipients)
                .HasForeignKey(r => r.AppMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
