using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGamesStore.Data;
using EasyGamesStore.Models;

namespace EasyGamesStore.Controllers
{
    // This controller manages all operations related to the owner’s main inventory.
    // It allows users with the "Owner" role to create, edit, view, and delete stock items.
    // The controller also calculates inventory summaries such as total value, revenue potential, and profit.
    [Authorize(Roles = "Owner")]
    public class StockItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // The constructor injects the application database context.
        // This provides access to the StockItems table and enables database operations.
        public StockItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays a list of all stock items in the owner’s main inventory.
        // Also calculates useful financial summaries for display at the top of the page.
        public async Task<IActionResult> Index()
        {
            // Retrieve all stock items from the database.
            var stockItems = await _context.StockItems.ToListAsync();

            // Calculate total inventory cost value (sum of all item cost * quantity).
            ViewBag.TotalInventoryValue = stockItems.Sum(s => s.CostPrice * s.Quantity);

            // Calculate total potential revenue if all items were sold at their selling price.
            ViewBag.TotalPotentialRevenue = stockItems.Sum(s => s.Price * s.Quantity);

            // Calculate total potential profit (difference between price and cost, multiplied by quantity).
            ViewBag.TotalPotentialProfit = stockItems.Sum(s => (s.Price - s.CostPrice) * s.Quantity);

            // Return the stock list to the view.
            return View(stockItems);
        }

        // Displays detailed information for a single stock item based on its ID.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Retrieve the specific item from the database.
            var stockItem = await _context.StockItems.FirstOrDefaultAsync(m => m.Id == id);
            if (stockItem == null)
                return NotFound();

            // Pass the item details to the view.
            return View(stockItem);
        }

        // Displays a form for creating a new stock item record in the inventory.
        public IActionResult Create()
        {
            return View();
        }

        // Handles submission of the form to create a new stock item.
        // Validates the form input before adding it to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Category,Price,Quantity,ImageUrl,IsNew,IsOnSale,OriginalPrice,ShortDescription,AverageRating,ReviewCount,CostPrice,Source")] StockItem stockItem)
        {
            // If all form fields are valid according to the model validation attributes.
            if (ModelState.IsValid)
            {
                // Add the new stock item to the database.
                _context.Add(stockItem);
                await _context.SaveChangesAsync();

                // Display a success message and redirect back to the index page.
                TempData["Message"] = $"Item '{stockItem.Title}' added successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, return the same form with validation errors.
            return View(stockItem);
        }

        // Displays the edit form for an existing stock item.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Retrieve the item to be edited from the database.
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null)
                return NotFound();

            return View(stockItem);
        }

        // Handles form submission for updating an existing stock item.
        // Updates all editable fields and saves changes to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Category,Price,Quantity,ImageUrl,IsNew,IsOnSale,OriginalPrice,ShortDescription,AverageRating,ReviewCount,CostPrice,Source")] StockItem stockItem)
        {
            // Ensure that the route ID matches the item being edited.
            if (id != stockItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the stock item record and save to the database.
                    _context.Update(stockItem);
                    await _context.SaveChangesAsync();

                    // Provide a success message once the update completes.
                    TempData["Message"] = $"'{stockItem.Title}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle potential concurrency issues if another process modifies the same record.
                    if (!_context.StockItems.Any(e => e.Id == stockItem.Id))
                        return NotFound();
                    else
                        throw;
                }
                // Redirect back to the list view after a successful update.
                return RedirectToAction(nameof(Index));
            }
            // If validation fails, redisplay the form.
            return View(stockItem);
        }

        // Displays a confirmation page before deleting a stock item.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            // Retrieve the item details for confirmation display.
            var stockItem = await _context.StockItems.FirstOrDefaultAsync(m => m.Id == id);
            if (stockItem == null)
                return NotFound();

            return View(stockItem);
        }

        // Handles confirmation and execution of the delete action.
        // Removes the stock item from the inventory permanently.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the record to delete.
            var stockItem = await _context.StockItems.FindAsync(id);

            if (stockItem != null)
            {
                _context.StockItems.Remove(stockItem);
                await _context.SaveChangesAsync();

                // Inform the user that the deletion was successful.
                TempData["Message"] = $"'{stockItem.Title}' deleted successfully.";
            }

            // Redirect to the index page after deletion.
            return RedirectToAction(nameof(Index));
        }
    }
}
