namespace EasyGamesStore.Models
{
    // This class represents a single item in a shopping cart.
    // It is used to store the details of items that a user wants to buy before checkout.
    public class CartItem
    {
        // The ID of the stock item this cart item represents.
        // This connects the cart item to the actual product in the database.
        public int StockItemId { get; set; }

        // The title or name of the product.
        // This is displayed in the cart so the user knows what they are buying.
        public string Title { get; set; } = "";

        // The price of a single unit of the product.
        // This is used to calculate totals in the cart and during checkout.
        public decimal Price { get; set; }

        // The quantity of this item in the cart.
        // Allows multiple units of the same product to be added.
        public int Quantity { get; set; }
    }
}
