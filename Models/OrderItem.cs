namespace EasyGamesStore.Models
{
    // This class represents a single product within an order.
    // Each OrderItem connects a specific product to a specific order and stores quantity and price information.
    public class OrderItem
    {
        // Unique ID for this order item. This acts as the primary key in the database.
        public int Id { get; set; }

        // The ID of the order this item belongs to.
        // This links the item to the parent order.
        public int OrderId { get; set; }

        // Navigation property to the Order object.
        // This allows access to the full order details if needed.
        public Order? Order { get; set; }

        // The ID of the stock item (product) included in the order.
        // This links the order item to the actual product in the inventory.
        public int StockItemId { get; set; }

        // Navigation property to the StockItem object.
        // Provides access to the full product details like title, price, etc.
        public StockItem? StockItem { get; set; }

        // Quantity of this product purchased in this order item.
        public int Quantity { get; set; }

        // Price per unit of this product at the time of order.
        // This allows calculating the total cost for this item in the order.
        public decimal UnitPrice { get; set; }
    }
}
