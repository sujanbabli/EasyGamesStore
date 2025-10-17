using System;
using System.Collections.Generic;

namespace EasyGamesStore.Models
{
    // Represents a Point-of-Sale (POS) order that is created at a physical shop location.
    // Each record stores information about the shop where the sale occurred, 
    // the customer involved (if applicable), the total amount, and the items purchased.
    public class ShopOrder
    {
        // Primary key of the ShopOrder table. Uniquely identifies each sale transaction.
        public int Id { get; set; }

        // The shop where the order was placed.
        // This establishes a relationship between the order and its corresponding shop.
        public int ShopId { get; set; }
        public Shop? Shop { get; set; }

        // The ID of the customer associated with this order.
        // If the purchase was made by a guest (not logged in), this remains null.
        public string? CustomerUserId { get; set; }

        // Optional field to record the customer’s phone number.
        // This is useful for identifying or linking purchases made by recurring customers.
        public string? CustomerPhone { get; set; }

        // The date and time when the order was created.
        // Defaults to the current UTC time for consistent tracking across all locations.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // The total amount of money paid for this order.
        // This includes all items purchased and any applicable discounts.
        public decimal Total { get; set; }

        // A collection of all the individual items included in this order.
        // Each entry in the list contains item details like quantity, price, and stock reference.
        public List<ShopOrderItem> Items { get; set; } = new();
    }
}
