using System;
using System.Collections.Generic;

namespace EasyGamesStore.Models
{
    // This class represents a customer's order in the system.
    // It is used to store information about what a user purchased and when.
    public class Order
    {
        // The unique ID for each order. This acts as the primary key in the database.
        public int Id { get; set; }

        // The ID of the user who placed the order.
        // This links the order to the specific customer.
        public string UserId { get; set; } = "";

        // The date and time when the order was created.
        // Default is set to current UTC time so we know exactly when the order was made.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // The total amount of the order.
        // This is calculated by summing the price * quantity of each item in the order.
        public decimal Total { get; set; }

        // A list of individual items in the order.
        // Each OrderItem represents a single product and its quantity in this order.
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
