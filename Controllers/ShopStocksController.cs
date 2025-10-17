using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGamesStore.Controllers
{
    // This controller manages the stock assigned to each shop owned or managed by a Proprietor.
    // It allows proprietors to view their shop inventory, add stock from the owner’s main inventory,
    // and automatically handle the deduction of quantities from the owner's main stock.
    // Access is restricted to users with Proprietor, Owner, or Admin roles.
    [Authorize(Roles = "Proprietor,Owner,Admin")]
    public class ShopStocksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // The constructor receives the application database context and user manager
        // to handle database access and user identity verification.
        public ShopStocksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays all stock items currently held in the logged-in proprietor’s shop.
        // Proprietors can only see stock assigned to their own shop.
        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;

            // Identify which shop belongs to the currently logged-in proprietor.
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == email);
            if (shop == null)
            {
                TempData["Error"] = "No shop is assigned to your account.";
                return RedirectToAction("Index", "Home");
            }

            // Retrieve all stock items linked to the proprietor’s shop, including full product details.
            var stock = await _context.ShopStocks
                .Include(s => s.StockItem)
                .Where(s => s.ShopId == shop.Id)
                .ToListAsync();

            // Pass shop name and stock data to the view for display.
            ViewBag.ShopName = shop.Name;
            return View(stock);
        }

        // Displays a form that allows the proprietor to add stock from the owner’s inventory to their shop.
        // The proprietor can view all available stock items owned by the system owner.
        public async Task<IActionResult> Create()
        {
            var email = User.Identity?.Name;
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == email);

            // If the logged-in user doesn’t have an assigned shop, redirect with an error.
            if (shop == null)
            {
                TempData["Error"] = "No shop is assigned to your account.";
                return RedirectToAction("Index");
            }

            // Retrieve the full list of stock items available in the owner’s inventory.
            var ownerStock = await _context.StockItems.ToListAsync();

            // Provide both the shop and owner stock list to the view.
            ViewBag.OwnerStock = ownerStock;
            ViewBag.Shop = shop;
            return View();
        }

        // Handles the submission of a new stock addition request.
        // The method deducts stock from the owner’s main inventory and adds it to the proprietor’s shop.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int stockItemId, int quantity)
        {
            var email = User.Identity?.Name;
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == email);

            // Check that the current user actually manages a valid shop.
            if (shop == null)
            {
                TempData["Error"] = "No shop is assigned to your account.";
                return RedirectToAction("Index");
            }

            // Retrieve the stock item from the owner’s main inventory.
            var stockItem = await _context.StockItems.FindAsync(stockItemId);
            if (stockItem == null)
            {
                TempData["Error"] = "Invalid stock item selected.";
                return RedirectToAction("Create");
            }

            // Validate that the requested quantity is a positive number.
            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than zero.";
                return RedirectToAction("Create");
            }

            // Ensure that the owner has enough stock available to transfer to the shop.
            if (stockItem.Quantity < quantity)
            {
                TempData["Error"] = $"Not enough stock available in Owner's inventory. " +
                                    $"Available: {stockItem.Quantity}, Requested: {quantity}.";
                return RedirectToAction("Create");
            }

            // Deduct the transferred quantity from the owner’s inventory.
            stockItem.Quantity -= quantity;
            _context.StockItems.Update(stockItem);

            // Check if the shop already has this product in its inventory.
            // If yes, increase its quantity; otherwise, create a new record.
            var existing = await _context.ShopStocks
                .FirstOrDefaultAsync(s => s.ShopId == shop.Id && s.StockItemId == stockItem.Id);

            if (existing != null)
            {
                // Update existing stock record with new quantity and inherited data.
                existing.Quantity += quantity;
                existing.PriceOverride = stockItem.Price;     // Inherit current selling price.
                existing.CostPrice = stockItem.CostPrice;     // Inherit base cost price.
                existing.Source = stockItem.Source;           // Inherit supplier/source info.
                _context.ShopStocks.Update(existing);
            }
            else
            {
                // Create a new stock record for this shop.
                var newShopStock = new ShopStock
                {
                    ShopId = shop.Id,
                    StockItemId = stockItem.Id,
                    Quantity = quantity,
                    PriceOverride = stockItem.Price,
                    CostPrice = stockItem.CostPrice,
                    Source = stockItem.Source
                };

                _context.ShopStocks.Add(newShopStock);
            }

            // Save all updates to the database.
            await _context.SaveChangesAsync();

            // Provide a clear summary message showing the transfer details and remaining stock.
            TempData["Message"] = $"{quantity} units of '{stockItem.Title}' have been added to your shop inventory. " +
                                  $"(Source: {stockItem.Source ?? "Unknown"}, Cost: ${stockItem.CostPrice}, " +
                                  $"Price: ${stockItem.Price}) — Remaining Owner stock: {stockItem.Quantity}.";

            return RedirectToAction("Index");
        }
    }
}
