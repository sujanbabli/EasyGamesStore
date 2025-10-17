using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGamesStore.Models
{
    // Represents a historical record of all purchases made by a specific user,
    // whether through the online store (Order) or a physical shop (ShopOrder).
    // This class is used for tracking customer spending, profit contributions, and purchase trends over time.
    public class UserSalesHistory
    {
        // Primary key for the UserSalesHistory table.
        // Each record corresponds to one completed transaction by a user.
        public int Id { get; set; }

        // The unique identifier of the user who made the purchase.
        // Links this record to the user account in the Identity system.
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Links the purchase to an online order (if applicable).
        // The property is nullable because not all sales come from the online system;
        // some are from physical shop POS transactions.
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        // Optional link to the ShopOrder table, representing in-store POS purchases.
        // This field allows the system to handle both online and offline transactions within the same history.
        public int? ShopOrderId { get; set; }
        public ShopOrder? ShopOrder { get; set; }

        // The total amount of money spent by the user in this transaction.
        // This helps calculate lifetime spending, loyalty tier, and customer value.
        public decimal TotalSpent { get; set; }

        // The total profit generated from this user’s purchase.
        // Calculated based on the difference between selling price and cost price for all items in the order.
        public decimal TotalProfit { get; set; }

        // The date and time when this purchase occurred.
        // Automatically recorded in UTC format for consistency across different time zones.
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }
}
