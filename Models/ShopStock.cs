using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGamesStore.Models
{
    // Represents inventory that belongs specifically to a Proprietor’s shop.
    // Each shop maintains its own stock records separate from the Owner’s central inventory (StockItems table).
    // This class helps track how many items each shop has in stock and their respective pricing details.
    public class ShopStock
    {
        // Primary key for the ShopStock table.
        // Each record represents a specific product stocked in a specific shop.
        public int Id { get; set; }

        // --- Relations ---

        // Foreign key linking this stock record to a particular shop.
        // Each shop can have multiple stock entries, but only one entry per product.
        [Required]
        public int ShopId { get; set; }
        public Shop? Shop { get; set; }

        // Foreign key linking to the original product (StockItem) from the main inventory.
        // This allows the shop’s stock to inherit item details such as title, category, or base price.
        [Required]
        public int StockItemId { get; set; }
        public StockItem? StockItem { get; set; }

        // --- Quantities ---

        // Indicates how many units of this product the shop currently has in stock.
        // The Range attribute ensures that negative quantities are not allowed.
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 0;

        // --- Inherited values from Owner’s inventory ---

        // The shop’s selling price for this product.
        // Typically starts as the Owner’s price but can be modified if the proprietor sets a local price.
        // The decimal precision ensures accurate handling of currency values.
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceOverride { get; set; }

        // The cost price for the product, inherited from the Owner’s inventory.
        // Used for calculating profits and evaluating shop performance.
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostPrice { get; set; }

        // Indicates the supplier or source from which the product originally came.
        // This field is inherited from the main StockItem record and helps with tracking product origins.
        [StringLength(100)]
        public string? Source { get; set; }
    }
}
