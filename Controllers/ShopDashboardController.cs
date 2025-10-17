using EasyGamesStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGamesStore.Controllers
{
    // This controller manages the shop dashboard that is accessible to Proprietors, Owners, and Admins.
    // It provides an overview of shop information and stock managed by a specific proprietor.
    // Role-based authorization ensures only these specific roles can view and manage shop details.
    [Authorize(Roles = "Proprietor,Owner,Admin")]
    public class ShopDashboardController : Controller
    {
        // The ApplicationDbContext provides access to the database entities such as Shops and ShopStocks.
        // UserManager is used to identify the currently logged-in user and access their account information.
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor that receives and stores the database context and user manager.
        // These dependencies are automatically injected by the ASP.NET Core dependency injection system.
        public ShopDashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the dashboard for the current proprietor.
        // The dashboard shows key details about their assigned shop and the inventory linked to that shop.
        public async Task<IActionResult> Index()
        {
            // The current user's email is retrieved from the logged-in identity.
            // This email is used to match the proprietor to their assigned shop.
            var email = User.Identity?.Name;

            // The system searches for a shop that has the same proprietor email as the logged-in user.
            // Each shop record has a ProprietorEmail field that associates it with a proprietor account.
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ProprietorEmail == email);

            // If no shop is found for the logged-in user, an error message is displayed,
            // and the user is redirected back to the home page. This prevents unauthorized access
            // or empty dashboards for unassigned accounts.
            if (shop == null)
            {
                TempData["Error"] = "No shop is assigned to your account. Please contact the Owner.";
                return RedirectToAction("Index", "Home");
            }

            // Retrieves all stock items that belong to this shop.
            // The ShopStocks table serves as a junction between Shops and StockItems,
            // meaning each record represents how many units of a specific stock item are available in a particular shop.
            var shopStock = await _context.ShopStocks
                .Include(s => s.StockItem)       // Includes related stock item details such as name, price, and category.
                .Where(s => s.ShopId == shop.Id) // Filters stock records that match the current shop's ID.
                .ToListAsync();

            // Calculates the total number of items currently held by this shop
            // by summing the quantity column across all stock records.
            int totalItems = shopStock.Sum(s => s.Quantity);

            // Passes the shop object, stock list, and total count to the view.
            // These values will be displayed on the dashboard page for the proprietor’s reference.
            ViewBag.Shop = shop;
            ViewBag.TotalItems = totalItems;
            ViewBag.ShopStock = shopStock;

            // Finally, returns the dashboard view to be rendered with the gathered data.
            return View();
        }
    }
}
