using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGamesStore.Models
{
    // Represents an item listed in the Owner’s main inventory and online store.
    // This model defines the products that the Owner sells directly to users or transfers to shop inventories.
    public class StockItem
    {
        // Primary key for the StockItem table. Each product in the system has a unique ID.
        public int Id { get; set; }

        // The name or title of the product, required for identification and display.
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        // The category this product belongs to (e.g., Games, Books, Toys).
        // Used for organizing and filtering products in the UI.
        [Required, StringLength(50)]
        public string Category { get; set; } = string.Empty;

        // The price at which the Owner sells this product.
        // Used as the default selling price for shops unless overridden.
        [Required, Range(0.01, 10000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // The number of units currently available in the Owner’s central inventory.
        // This quantity decreases when stock is transferred to shops or sold online.
        [Required, Range(0, 1000)]
        public int Quantity { get; set; }

        // An optional image URL for displaying the product visually on the website.
        public string? ImageUrl { get; set; }

        // Flags to help mark and filter items in the user interface.
        // "IsNew" identifies new arrivals, and "IsOnSale" marks discounted products.
        public bool IsNew { get; set; } = false;
        public bool IsOnSale { get; set; } = false;

        // Stores the original price of the product before a sale or discount is applied.
        // Useful for displaying the discounted percentage to users.
        [Range(0.01, 10000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal OriginalPrice { get; set; } = 0;

        // A short summary or tagline describing the product.
        // This helps customers understand what the item is at a glance.
        [StringLength(250)]
        public string? ShortDescription { get; set; }

        // Average customer rating (out of 5) based on user reviews.
        // Helps display popularity or satisfaction metrics on the storefront.
        [Range(0, 5)]
        public double AverageRating { get; set; } = 0;

        // Total number of reviews received for this product.
        // Combined with AverageRating, it gives insight into customer feedback.
        public int ReviewCount { get; set; } = 0;

        // --- NEW FOR ASSIGNMENT 3 ---

        // The cost price (or buying price) of the product, representing how much the Owner paid to acquire it.
        // This is used in profit calculations and performance reports.
        [Range(0, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostPrice { get; set; } = 0;

        // The supplier or source from which the product was obtained.
        // This helps trace where stock originated and manage supplier relationships.
        [StringLength(100)]
        public string? Source { get; set; }

        // A computed property that calculates the profit per unit.
        // It is not stored in the database but derived from Price and CostPrice whenever accessed.
        [NotMapped]
        public decimal ProfitPerUnit => Price > CostPrice ? (Price - CostPrice) : 0;
    }
}
