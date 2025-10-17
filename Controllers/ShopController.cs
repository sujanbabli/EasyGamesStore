using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGamesStore.Controllers
{
    // This controller manages the main shop operations including displaying items, managing the cart,
    // processing checkout, updating user tiers, and generating reports. 
    // The [Authorize] attribute ensures only authenticated users can access the shop.
    [Authorize]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // The constructor injects the application database context and user manager.
        // This allows access to stored data and user identity information throughout the controller.
        public ShopController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the main shop page, showing all available stock items to users.
        // Proprietor users are restricted from accessing this page and redirected to their stock management page.
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? category, string? search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && await _userManager.IsInRoleAsync(user, "Proprietor"))
            {
                TempData["Error"] = "You do not have permission to access the main Shop page.";
                return RedirectToAction("Index", "ShopStocks");
            }

            var items = await _context.StockItems.ToListAsync();

            // Adjust displayed quantities based on what the user currently has in their cart.
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            foreach (var item in items)
            {
                var inCart = cart.FirstOrDefault(c => c.StockItemId == item.Id);
                if (inCart != null)
                    item.Quantity -= inCart.Quantity;
            }

            // Apply category and search filters if provided.
            if (!string.IsNullOrEmpty(category))
                items = items.Where(i => i.Category == category).ToList();

            if (!string.IsNullOrEmpty(search))
                items = items.Where(i => i.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;

            return View(items);
        }

        // Allows a regular user to clear their cart completely.
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Message"] = "Your cart has been cleared.";
            return RedirectToAction("Cart");
        }

        // Adds a selected stock item to the user’s cart, while checking available quantity.
        [Authorize(Roles = "User")]
        public IActionResult AddToCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = _context.StockItems.Find(id);

            if (item != null)
            {
                var inCart = cart.FirstOrDefault(c => c.StockItemId == id);
                int available = item.Quantity - (inCart?.Quantity ?? 0);

                // Prevents adding more than available quantity.
                if (available <= 0)
                {
                    TempData["Error"] = $"{item.Title} is out of stock for your cart.";
                    return RedirectToAction("Index");
                }

                if (inCart != null)
                    inCart.Quantity++;
                else
                    cart.Add(new CartItem
                    {
                        StockItemId = item.Id,
                        Title = item.Title,
                        Price = item.Price,
                        Quantity = 1
                    });

                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Cart");
        }

        // Displays the current contents of the user’s cart.
        [Authorize(Roles = "User")]
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // Updates the quantity of an item in the cart via an AJAX request (for plus or minus buttons).
        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult UpdateQuantityAjax(int id, string change)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.StockItemId == id);
            string? message = null;

            if (item != null)
            {
                var stockItem = _context.StockItems.Find(id);

                if (change == "plus")
                {
                    if (stockItem != null && item.Quantity < stockItem.Quantity)
                        item.Quantity++;
                    else if (stockItem != null)
                        message = $"Cannot add more than {stockItem.Quantity} items for \"{item.Title}\".";
                }
                else if (change == "minus")
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                        cart.Remove(item);
                }

                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            var cartCount = cart.Sum(c => c.Quantity);
            return Json(new { cartCount, message });
        }

        // Returns the total number of items in the user’s cart for live updates on the navigation bar.
        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartCount = cart.Sum(c => c.Quantity);
            return Json(cartCount);
        }

        // Handles the checkout process. Validates stock availability, creates the order, and updates user tiers.
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
                return RedirectToAction("Index");

            foreach (var cartItem in cart)
            {
                var stockItem = await _context.StockItems.FindAsync(cartItem.StockItemId);
                if (stockItem == null || stockItem.Quantity < cartItem.Quantity)
                {
                    TempData["Error"] = $"Not enough stock for {cartItem.Title}.";
                    return RedirectToAction("Cart");
                }
            }

            var userId = _userManager.GetUserId(User);

            var order = new Order
            {
                UserId = userId!,
                CreatedAt = DateTime.UtcNow,
                Total = cart.Sum(c => c.Price * c.Quantity),
                Items = cart.Select(c => new OrderItem
                {
                    StockItemId = c.StockItemId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                }).ToList()
            };

            decimal totalProfit = 0;

            // Reduce stock quantities and calculate total profit for the order.
            foreach (var cartItem in cart)
            {
                var stockItem = await _context.StockItems.FindAsync(cartItem.StockItemId);
                if (stockItem != null)
                {
                    stockItem.Quantity -= cartItem.Quantity;
                    totalProfit += (stockItem.Price - stockItem.CostPrice) * cartItem.Quantity;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Record user purchase and profit data in the sales history table.
            _context.UserSalesHistories.Add(new UserSalesHistory
            {
                UserId = userId!,
                OrderId = order.Id,
                TotalSpent = order.Total,
                TotalProfit = totalProfit,
                PurchaseDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await UpdateUserTier(userId!);
            HttpContext.Session.Remove("Cart");

            return View("OrderConfirmation", order);
        }

        // Updates the user’s tier based on their total profit contribution to the shop.
        // Tiers are Bronze, Silver, Gold, and Platinum.
        private async Task UpdateUserTier(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
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

            var existingClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "Tier");
            if (existingClaim != null)
                await _userManager.RemoveClaimAsync(user, existingClaim);

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Tier", newTier));
        }

        // Clears all purchase history records for the current user.
        [HttpPost]
        public async Task<IActionResult> ClearUserHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var histories = _context.UserSalesHistories.Where(h => h.UserId == user.Id);
            _context.UserSalesHistories.RemoveRange(histories);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Updates a user’s bio information and stores it as a claim.
        // This can later be retrieved or persisted in the database.
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> UpdateBio([FromBody] dynamic data)
        {
            string bio = data?.bio ?? "";
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var claims = await _userManager.GetClaimsAsync(user);
            var existingBio = claims.FirstOrDefault(c => c.Type == "Bio");
            if (existingBio != null)
                await _userManager.RemoveClaimAsync(user, existingBio);

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Bio", bio));
            return Content(bio);
        }

        // Generates and downloads a PDF purchase report summarizing the user’s orders, totals, and taxes.
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> DownloadReport()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userId = user.Id;

            var histories = await _context.UserSalesHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PurchaseDate)
                .ToListAsync();

            var totalOrders = histories.Count;
            var totalSpent = histories.Sum(h => h.TotalSpent);
            var totalTax = totalSpent * 0.10m;
            var totalWithTax = totalSpent + totalTax;

            var claims = await _userManager.GetClaimsAsync(user);
            var userTier = claims.FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";

            using var ms = new MemoryStream();

            var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            var title = new iText.Layout.Element.Paragraph("EasyGamesStore Purchase Report")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetMarginBottom(10);
            doc.Add(title);

            doc.Add(new iText.Layout.Element.Paragraph($"Name: {user.UserName}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Email: {user.Email}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Tier: {userTier}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Total Orders: {totalOrders}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Report Generated: {DateTime.UtcNow:dd MMM yyyy, hh:mm tt}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph(" "));

            var table = new iText.Layout.Element.Table(4).UseAllAvailableWidth();
            table.AddHeaderCell("Order ID");
            table.AddHeaderCell("Date");
            table.AddHeaderCell("Subtotal ($)");
            table.AddHeaderCell("Total (with Tax $)");

            const decimal taxRate = 0.10m;
            foreach (var record in histories)
            {
                var subtotal = record.TotalSpent;
                var withTax = subtotal * (1 + taxRate);

                table.AddCell(record.OrderId.ToString());
                table.AddCell(record.PurchaseDate.ToShortDateString());
                table.AddCell(subtotal.ToString("N2"));
                table.AddCell(withTax.ToString("N2"));
            }

            doc.Add(table);

            doc.Add(new iText.Layout.Element.Paragraph(" "));
            doc.Add(new iText.Layout.Element.Paragraph("Payment Summary").SetFont(boldFont).SetFontSize(14));
            doc.Add(new iText.Layout.Element.Paragraph($"Subtotal (before tax): ${totalSpent:N2}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Tax (10%): ${totalTax:N2}").SetFont(regularFont));
            doc.Add(new iText.Layout.Element.Paragraph($"Total with Tax: ${totalWithTax:N2}").SetFont(boldFont).SetFontSize(12));

            doc.Close();

            return File(ms.ToArray(), "application/pdf", "MyPurchaseReport.pdf");
        }

        // Displays a personalized dashboard with a user's sales history, tier, and spending summary.
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MySalesHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.StockItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            const decimal taxRate = 0.10m;

            var orderSummaries = orders.Select(o => new
            {
                o.Id,
                o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.StockItem?.Title,
                    UnitPrice = i.UnitPrice,
                    i.Quantity,
                    Subtotal = Math.Round(i.UnitPrice * i.Quantity, 2)
                }).ToList(),
                TotalBeforeTax = Math.Round(o.Total, 2),
                TaxAmount = Math.Round(o.Total * taxRate, 2),
                TotalAfterTax = Math.Round(o.Total * (1 + taxRate), 2)
            }).ToList();

            var totalSpent = orderSummaries.Sum(o => o.TotalAfterTax);
            var totalOrders = orderSummaries.Count;
            var avgSpent = totalOrders > 0 ? totalSpent / totalOrders : 0;

            string userTier = totalSpent switch
            {
                >= 10000 => "Platinum",
                >= 5000 => "Gold",
                >= 2000 => "Silver",
                _ => "Bronze"
            };

            ViewBag.UserName = user?.UserName ?? "Customer";
            ViewBag.UserEmail = user?.Email ?? "Unknown";
            ViewBag.UserTier = userTier;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AvgSpent = avgSpent;
            ViewBag.OrderSummaries = orderSummaries;

            return View();
        }

        // Generates a report for the Owner showing total revenue, profit, and top-performing users.
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UserProfitReport()
        {
            // Join UserSalesHistories with AspNetUsers to get Email
            var report = await _context.UserSalesHistories
                .Join(_context.Users,
                      history => history.UserId,
                      user => user.Id,
                      (history, user) => new { history, user.Email })
                .GroupBy(x => new { x.history.UserId, x.Email })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    Email = g.Key.Email,
                    TotalProfit = g.Sum(x => x.history.TotalProfit),
                    TotalSpent = g.Sum(x => x.history.TotalSpent),
                    Orders = g.Count()
                })
                .OrderByDescending(g => g.TotalProfit)
                .ToListAsync();

            // ✅ FIX: Add all ViewBag values after verifying report list
            ViewBag.TotalUsers = report.Count; // ← This is what feeds your summary card
            ViewBag.TotalRevenue = report.Any() ? report.Sum(r => r.TotalSpent) : 0m;
            ViewBag.TotalProfit = report.Any() ? report.Sum(r => r.TotalProfit) : 0m;
            ViewBag.AvgProfitMargin = ViewBag.TotalRevenue > 0
                ? (decimal)ViewBag.TotalProfit / (decimal)ViewBag.TotalRevenue * 100
                : 0m;
            ViewBag.TopUsers = report.Take(3).ToList();

            return View(report);
        }

    }
}
//The ShopController manages all user and owner activities related to the store,
//including viewing items, adding and updating the shopping cart, completing purchases,
//and generating reports. The controller ensures proper role-based access, preventing
//proprietors from using the public shop and allowing only authenticated users to make purchases.
//Each checkout updates product stock, records profits, and adjusts user tiers based on cumulative profit.
//Additional methods handle clearing user history, updating bio information, and
//generating detailed PDF purchase reports. The owner can also view a summarized profit report
//that includes revenue, total profit, and the top three users based on contribution.
//The design balances functionality, security, and clear role separation between users, proprietors, and owners.