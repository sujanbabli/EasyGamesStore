using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EasyGamesStore.Controllers
{
    // This controller is responsible for managing shop records and assigning proprietors to specific shops.
    // Only users with the "Owner" or "Admin" role are authorized to access these actions.
    // The controller provides full CRUD functionality for shops (create, read, update, delete),
    // and includes a dedicated section for assigning proprietors (shop managers) to shops.
    [Authorize(Roles = "Owner,Admin")]
    public class ShopsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // The constructor injects three services:
        // 1. ApplicationDbContext for database access,
        // 2. UserManager to manage user accounts,
        // 3. RoleManager to manage user roles (like "Proprietor").
        public ShopsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ======================================
        //  SECTION 1: SHOP CRUD (Create, Read, Update, Delete)
        // ======================================

        // Displays a list of all shops in the system.
        public async Task<IActionResult> Index()
        {
            var shops = await _context.Shops.ToListAsync();
            return View(shops);
        }

        // Displays detailed information for a specific shop by ID.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var shop = await _context.Shops.FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
                return NotFound();

            return View(shop);
        }

        // Displays the form for creating a new shop record.
        public IActionResult Create() => View();

        // Handles the creation of a new shop record in the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Phone")] Shop shop)
        {
            // If the form validation fails, redisplay the form with entered data.
            if (!ModelState.IsValid)
                return View(shop);

            // Add the new shop record to the database.
            _context.Add(shop);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Shop created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Displays the edit form for an existing shop record.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
                return NotFound();

            return View(shop);
        }

        // Handles the submission of updated shop data.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shop shop)
        {
            // Ensure that the shop ID in the URL matches the one in the form data.
            if (id != shop.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the existing shop record and save the changes.
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Shop updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle the case where the record no longer exists (deleted by another process).
                    if (!ShopExists(shop.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        // Displays the confirmation page before deleting a shop.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var shop = await _context.Shops.FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
                return NotFound();

            return View(shop);
        }

        // Confirms and executes the deletion of a shop record.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Shop deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Utility function that checks whether a shop exists in the database by its ID.
        private bool ShopExists(int id) => _context.Shops.Any(e => e.Id == id);


        // ======================================
        //  SECTION 2: ASSIGN PROPRIETOR TO SHOP
        // ======================================

        // Displays a list of unassigned shops (shops without proprietors) to select from.
        [HttpGet]
        public async Task<IActionResult> AssignProprietor()
        {
            // Fetch shops that do not have a proprietor assigned yet.
            var shops = await _context.Shops
                .Where(s => s.ProprietorUserId == null)
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Pass the list of shops to the view using ViewBag.
            ViewBag.Shops = shops;
            return View();
        }

        // Handles the form submission for assigning a proprietor to a shop.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProprietor(string proprietorEmail, string proprietorPassword, int shopId)
        {
            // Basic validation: Ensure all required fields are filled.
            if (string.IsNullOrWhiteSpace(proprietorEmail) || string.IsNullOrWhiteSpace(proprietorPassword) || shopId == 0)
            {
                ModelState.AddModelError("", "All fields are required.");
            }

            // Retrieve the selected shop from the database.
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null)
            {
                ModelState.AddModelError("", "Shop not found.");
            }

            // If there are validation errors, reload the list of shops and return to the form.
            if (!ModelState.IsValid)
            {
                ViewBag.Shops = await _context.Shops
                    .Where(s => s.ProprietorUserId == null)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                return View();
            }

            // Check if a user with the provided email already exists.
            var existingUser = await _userManager.FindByEmailAsync(proprietorEmail);

            // If not found, create a new IdentityUser account for the proprietor.
            IdentityUser user = existingUser ?? new IdentityUser
            {
                UserName = proprietorEmail,
                Email = proprietorEmail,
                EmailConfirmed = true
            };

            // If the user did not exist before, create them with the given password.
            if (existingUser == null)
            {
                var result = await _userManager.CreateAsync(user, proprietorPassword);
                if (!result.Succeeded)
                {
                    // If creation fails, show detailed error messages from Identity.
                    ModelState.AddModelError("", "Error creating user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    ViewBag.Shops = await _context.Shops.ToListAsync();
                    return View();
                }
            }

            // Ensure that the "Proprietor" role exists in the system before assigning it.
            if (!await _roleManager.RoleExistsAsync("Proprietor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Proprietor"));
            }

            // Assign the "Proprietor" role to the user.
            await _userManager.AddToRoleAsync(user, "Proprietor");

            // Link the proprietor to the selected shop by storing the user’s ID and email in the shop record.
            shop.ProprietorUserId = user.Id;
            shop.ProprietorEmail = user.Email;

            _context.Update(shop);
            await _context.SaveChangesAsync();

            // Display a success message and redirect to the list of shops.
            TempData["Message"] = $"Proprietor {proprietorEmail} has been successfully assigned to the shop '{shop.Name}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
