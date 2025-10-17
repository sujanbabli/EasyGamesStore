using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyGamesStore.Controllers
{
    // This controller handles the Point-of-Sale (POS) system for each shop.
    // Only users with the roles "Proprietor", "Owner", or "Admin" are allowed access.
    // It allows proprietors to view their stock, process in-store sales, issue receipts,
    // and automatically manage customer tiers and purchase history.
    [Authorize(Roles = "Proprietor,Owner,Admin")]
    public class ShopPOSController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor injecting database context and user manager.
        // These services are required to access shop records and manage user accounts.
        public ShopPOSController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the main POS page where the proprietor can see available stock and make sales.
        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == email);

            // If no shop is assigned to this account, redirect with an error message.
            if (shop == null)
            {
                TempData["Error"] = "No shop assigned to your account.";
                return RedirectToAction("Index", "Home");
            }

            // Retrieve all stock items linked to this shop along with their product details.
            var stock = await _context.ShopStocks
                .Include(s => s.StockItem)
                .Where(s => s.ShopId == shop.Id)
                .ToListAsync();

            ViewBag.Shop = shop;
            return View(stock);
        }

        // Returns customer tier and discount information using email or phone lookup.
        // This helps proprietors apply appropriate discounts during checkout.
        [HttpGet]
        public async Task<IActionResult> GetCustomerTier(string emailOrPhone)
        {
            if (string.IsNullOrWhiteSpace(emailOrPhone))
                return Json(new { found = false });

            // Find user by email, username, or a guest email pattern.
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == emailOrPhone ||
                    u.UserName == emailOrPhone ||
                    u.Email == $"{emailOrPhone}@guest.local");

            if (user == null)
                return Json(new { found = false });

            // Retrieve the tier claim and determine applicable discount.
            var tierClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";

            var discount = tierClaim switch
            {
                "Silver" => 5,
                "Gold" => 10,
                "Platinum" => 15,
                _ => 0
            };

            return Json(new { found = true, tier = tierClaim, discount });
        }

        // Handles the sale transaction process when the proprietor completes a purchase in POS.
        // This includes updating stock quantities, creating order records,
        // updating user history, and applying discounts based on tier.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSale(
            string? customerPhone,
            string? customerEmail,
            bool signupNew,
            Dictionary<int, int> quantities)
        {
            var proprietorEmail = User.Identity?.Name;
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == proprietorEmail);

            if (shop == null)
            {
                TempData["Error"] = "No shop assigned to your account.";
                return RedirectToAction("Index");
            }

            var orderItems = new List<ShopOrderItem>();
            decimal total = 0;
            decimal totalProfit = 0;
            var warnings = new List<string>();

            // For each product in the sale, verify stock, calculate totals, and deduct sold quantities.
            foreach (var kv in quantities)
            {
                int stockId = kv.Key;
                int qty = kv.Value;
                if (qty <= 0) continue;

                var shopStock = await _context.ShopStocks
                    .Include(s => s.StockItem)
                    .FirstOrDefaultAsync(s => s.ShopId == shop.Id && s.StockItemId == stockId);

                if (shopStock == null || shopStock.StockItem == null)
                    continue;

                var price = shopStock.PriceOverride ?? shopStock.StockItem.Price;
                var cost = shopStock.StockItem.CostPrice;
                total += price * qty;
                totalProfit += (price - cost) * qty;

                orderItems.Add(new ShopOrderItem
                {
                    StockItemId = stockId,
                    Quantity = qty,
                    UnitPrice = price
                });

                shopStock.Quantity -= qty;
                _context.ShopStocks.Update(shopStock);

                // Warn the proprietor if stock is running low.
                if (shopStock.Quantity <= 2)
                    warnings.Add($"Low stock for {shopStock.StockItem.Title} (Remaining: {shopStock.Quantity}).");
            }

            // Prevents creating an empty order if no valid items were selected.
            if (!orderItems.Any())
            {
                TempData["Error"] = "No items selected for sale.";
                return RedirectToAction("Index");
            }

            await _context.SaveChangesAsync();

            // Find existing customer or create a new guest user if necessary.
            IdentityUser? customer = null;
            string? customerUserId = null;

            if (!string.IsNullOrWhiteSpace(customerEmail) || !string.IsNullOrWhiteSpace(customerPhone))
            {
                string lookupKey = customerEmail ?? $"{customerPhone}@guest.local";

                customer = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == lookupKey || u.UserName == lookupKey);

                if (customer == null && signupNew)
                {
                    customer = new IdentityUser
                    {
                        UserName = lookupKey,
                        Email = lookupKey,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(customer, "Guest@1234");
                    if (!result.Succeeded)
                    {
                        TempData["Error"] = "Could not create new customer.";
                        return RedirectToAction("Index");
                    }
                }

                if (customer != null)
                    customerUserId = customer.Id;
            }

            // Determine discount rate based on user tier.
            decimal discountRate = 0;
            string currentTier = "Bronze";

            if (customerUserId != null)
            {
                var tierClaim = (await _userManager.GetClaimsAsync(customer!))
                    .FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";

                currentTier = tierClaim;
                discountRate = tierClaim switch
                {
                    "Silver" => 0.05m,
                    "Gold" => 0.10m,
                    "Platinum" => 0.15m,
                    _ => 0
                };
            }

            // Apply discount to total price.
            decimal discountAmount = total * discountRate;
            total -= discountAmount;

            // Create a ShopOrder record for the transaction.
            var order = new ShopOrder
            {
                ShopId = shop.Id,
                CustomerUserId = customerUserId,
                CustomerPhone = customerPhone,
                CreatedAt = DateTime.UtcNow,
                Total = total,
                Items = orderItems
            };

            _context.ShopOrders.Add(order);
            await _context.SaveChangesAsync();

            // Record sale history for the user and update tier based on accumulated profit.
            if (customerUserId != null)
            {
                _context.UserSalesHistories.Add(new UserSalesHistory
                {
                    UserId = customerUserId,
                    ShopOrderId = order.Id,
                    TotalSpent = total,
                    TotalProfit = totalProfit,
                    PurchaseDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await UpdateUserTier(customerUserId);
            }

            // Display warnings for any low-stock items after the sale.
            if (warnings.Any())
                TempData["Warning"] = string.Join("<br>", warnings);

            // Confirmation message summarizing sale details.
            TempData["Message"] = $"Sale completed! Total after discount: ${total:0.00}. Current tier: {currentTier}";
            return RedirectToAction("Receipt", new { id = order.Id });
        }

        // Displays the sale receipt with order details and customer information.
        public async Task<IActionResult> Receipt(int id)
        {
            var order = await _context.ShopOrders
                .Include(o => o.Items).ThenInclude(i => i.StockItem)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            string? tier = null;
            string? customerEmail = null;

            if (order.CustomerUserId != null)
            {
                var user = await _userManager.FindByIdAsync(order.CustomerUserId);
                if (user != null)
                {
                    customerEmail = user.Email;
                    tier = (await _userManager.GetClaimsAsync(user))
                        .FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";
                }
            }

            ViewBag.Tier = tier;
            ViewBag.CustomerEmail = customerEmail;
            return View(order);
        }

        // Updates a customer’s loyalty tier based on their total accumulated profit.
        // This ensures loyalty rewards and discounts stay up-to-date for future purchases.
        private async Task UpdateUserTier(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var totalProfit = await _context.UserSalesHistories
                .Where(h => h.UserId == userId)
                .SumAsync(h => h.TotalProfit);

            string newTier = totalProfit switch
            {
                >= 10000 => "Platinum",
                >= 5000 => "Gold",
                >= 2000 => "Silver",
                _ => "Bronze"
            };

            var existingClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "Tier");
            if (existingClaim != null)
                await _userManager.RemoveClaimAsync(user, existingClaim);

            await _userManager.AddClaimAsync(user, new Claim("Tier", newTier));
        }
    }
}


//The ShopPOSController manages all Point-of-Sale (POS) operations for proprietors and admins.
//It allows staff to view available stock, process walk-in or phone sales, apply automatic
//tier-based discounts, and generate receipts for customers. When a sale is processed,
//the system checks stock availability, deducts sold quantities, calculates profit,
//and updates the customer’s purchase history. If the customer doesn’t exist in the database,
//a temporary guest account can be created. The controller also adjusts user tiers dynamically
//based on total profit, rewarding loyal customers with increasing discounts (Silver, Gold, Platinum).
//The receipt view displays detailed transaction data along with the customer’s email and current tier.
//Overall, this controller ties together shop operations, user management, and sales history in a single,
//efficient POS workflow.