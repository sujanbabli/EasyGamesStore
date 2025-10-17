using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGamesStore.Controllers
{
    // This controller manages the transfer of stock items from the Owner’s main inventory 
    // to individual shops managed by proprietors. 
    // Only users with the "Owner" or "Admin" roles can perform these transfers.
    [Authorize(Roles = "Owner,Admin")]
    public class ShopTransfersController : Controller
    {
        private readonly ApplicationDbContext _context;

        // The ApplicationDbContext is injected to allow database access and operations.
        public ShopTransfersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays a complete list of all past stock transfers that have been made from the owner 
        // to different shops. Each record shows which item was transferred, to which shop, and when.
        public async Task<IActionResult> Index()
        {
            // Retrieves all transfer records from the database and includes related shop and stock details.
            var transfers = await _context.ShopTransfers
                .Include(t => t.Shop)
                .Include(t => t.StockItem)
                .OrderByDescending(t => t.CreatedAt) // Most recent transfers appear first.
                .ToListAsync();

            // Passes the transfer list to the view for display.
            return View(transfers);
        }

        // Displays the form that allows the Owner/Admin to create a new transfer record.
        // The form includes dropdowns for selecting a shop and a stock item.
        public async Task<IActionResult> Create()
        {
            // Load all shops and available stock items from the database 
            // so they can be displayed in the selection list on the form.
            ViewBag.Shops = await _context.Shops.ToListAsync();
            ViewBag.StockItems = await _context.StockItems.ToListAsync();
            return View();
        }

        // Handles the submission of a new transfer request.
        // Validates the entered quantity, ensures there is enough stock in the main inventory,
        // updates both the owner’s and shop’s stock, and records the transfer.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int shopId, int stockItemId, int quantity)
        {
            // Validation step 1: Check that the transfer quantity is a positive number.
            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than zero.";
                return RedirectToAction(nameof(Create));
            }

            // Retrieve the selected stock item from the owner’s main inventory.
            var stockItem = await _context.StockItems.FindAsync(stockItemId);
            if (stockItem == null)
            {
                TempData["Error"] = "Stock item not found.";
                return RedirectToAction(nameof(Create));
            }

            // Validation step 2: Ensure the owner has enough stock to complete the transfer.
            if (stockItem.Quantity < quantity)
            {
                TempData["Error"] = $"Not enough stock in main inventory. Available: {stockItem.Quantity}.";
                return RedirectToAction(nameof(Create));
            }

            // Deduct the transferred quantity from the main inventory to maintain accuracy.
            stockItem.Quantity -= quantity;

            // Check if the selected shop already has a record for this stock item.
            // If the item exists, increase its quantity. If not, create a new record for it.
            var shopStock = await _context.ShopStocks
                .FirstOrDefaultAsync(s => s.ShopId == shopId && s.StockItemId == stockItemId);

            if (shopStock == null)
            {
                // If this is the first time the item is being sent to this shop,
                // a new ShopStock record is created with the initial quantity.
                shopStock = new ShopStock
                {
                    ShopId = shopId,
                    StockItemId = stockItemId,
                    Quantity = 0,
                    PriceOverride = stockItem.Price // Set selling price to match owner’s current price.
                };
                _context.ShopStocks.Add(shopStock);
            }

            // Add the transferred quantity to the shop’s stock record.
            shopStock.Quantity += quantity;

            // Create a new ShopTransfer record to maintain a historical log of the transaction.
            var transfer = new ShopTransfer
            {
                ShopId = shopId,
                StockItemId = stockItemId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };
            _context.ShopTransfers.Add(transfer);

            // Save all changes to the database — including the updated stock quantities and the new transfer log.
            await _context.SaveChangesAsync();

            // Display a confirmation message indicating the transfer was successful.
            TempData["Message"] = $"Successfully transferred {quantity} units of '{stockItem.Title}' to the selected shop.";
            return RedirectToAction(nameof(Index));
        }
    }
}
