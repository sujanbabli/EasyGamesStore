using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGamesStore.Models
{
    // Represents an individual item included in a shop order.
    // Each ShopOrderItem links a specific product (StockItem) to its order (ShopOrder),
    // along with quantity, price, and computed profit details.
    public class ShopOrderItem
    {
        // Primary key of the ShopOrderItem table. Each record represents one product in an order.
        public int Id { get; set; }

        // Foreign key linking this item to its parent ShopOrder.
        // This establishes a one-to-many relationship between ShopOrder and ShopOrderItem.
        public int ShopOrderId { get; set; }
        public ShopOrder? ShopOrder { get; set; }

        // Foreign key linking this item to a StockItem (the product being sold).
        // This allows the system to pull product details such as title, cost, and category.
        public int StockItemId { get; set; }
        public StockItem? StockItem { get; set; }

        // The number of units of this product sold in the order.
        public int Quantity { get; set; }

        // The price per unit that the customer paid for this product at the time of sale.
        // This value may differ from the default price if discounts or overrides were applied.
        public decimal UnitPrice { get; set; }

        // A computed property that calculates profit for this product line.
        // It subtracts the item's cost price (from StockItem) from the selling price (UnitPrice)
        // and multiplies by the number of units sold.
        // The [NotMapped] attribute ensures this value is not stored in the database but is calculated dynamically when accessed.
        [NotMapped]
        public decimal Profit => (StockItem == null) ? 0 :
            (UnitPrice > StockItem.CostPrice ? (UnitPrice - StockItem.CostPrice) * Quantity : 0);
    }
}
