using System;

namespace EasyGamesStore.Models
{
    // Records movement of stock from the main owner inventory to a shop.
    public class ShopTransfer
    {
        public int Id { get; set; }

        // Which shop the items are going to
        public int ShopId { get; set; }
        public Shop? Shop { get; set; }   // ✅ navigation property added

        // Which stock item is being transferred
        public int StockItemId { get; set; }
        public StockItem? StockItem { get; set; }   // ✅ navigation property added

        // Quantity of items moved
        public int Quantity { get; set; }

        // When it happened
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Who performed the transfer (Owner/Admin)
        public string? PerformedByUserId { get; set; }
    }
}
