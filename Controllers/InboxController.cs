using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyGamesStore.Controllers
{
    // This controller manages the inbox section for normal users.
    // Only users with the "User" role can access this controller.
    // It allows users to view messages sent by the Owner and mark them as read.
    [Authorize(Roles = "User")]
    public class InboxController : Controller
    {
        // The database context allows access to tables such as AppMessage and AppMessageRead.
        // UserManager is used to identify which user is currently logged in.
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor for injecting the database context and user manager.
        // Dependency injection provides these services automatically.
        public InboxController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // This action displays the user's inbox page.
        // It supports filtering messages by category (e.g., Promotion, Update, etc.)
        // and searching through messages using a keyword.
        public async Task<IActionResult> Index(string filter = "All", string? search = null)
        {
            // Get the currently logged-in user’s ID.
            var userId = _userManager.GetUserId(User);

            // Start building a query that retrieves messages belonging to this user.
            var query = _context.AppMessageReads
                .Include(r => r.AppMessage) // Include message details.
                .Where(r => r.UserId == userId) // Filter only messages assigned to this user.
                .OrderByDescending(r => r.AppMessage!.CreatedAt) // Show the most recent messages first.
                .AsQueryable();

            // If a specific filter is selected (other than "All"), apply it.
            // The filter corresponds to message categories defined in the MessageCategory enum.
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<MessageCategory>(filter, true, out var cat))
                    query = query.Where(r => r.AppMessage!.Category == cat);
            }

            // Apply a search filter if a search keyword is provided.
            // It looks for matches in both the title and the body of the message.
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.AppMessage!.Title.Contains(search) || r.AppMessage!.HtmlBody.Contains(search));

            // Execute the query and retrieve the final list of messages.
            var items = await query.ToListAsync();

            // Count how many messages are unread for this user.
            // This count is shown both in the inbox page and the navigation bar badge.
            int unreadCount = await _context.AppMessageReads
                .CountAsync(r => r.UserId == userId && !r.IsRead);

            // Store view data for filters, search, and unread counts.
            ViewBag.Filter = filter;
            ViewBag.Search = search;
            ViewBag.UnreadCount = unreadCount;            // Displayed in the inbox page
            ViewBag.UnreadMessagesCount = unreadCount;    // Displayed in the navbar badge

            // Pass the final list of messages to the view.
            return View(items);
        }

        // This action marks a single message as read.
        // It is triggered when the user opens or manually marks a message as read.
        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Locate the message read record for this user by ID.
            var row = await _context.AppMessageReads.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            // If found and still unread, mark it as read and set the read timestamp.
            if (row != null && !row.IsRead)
            {
                row.IsRead = true;
                row.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Return an OK response to confirm the update.
            return Ok();
        }

        // This action marks all messages in the user’s inbox as read.
        // It is useful for users who want to clear their inbox quickly.
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User);

            // Retrieve all unread messages for this user.
            var rows = await _context.AppMessageReads.Where(r => r.UserId == userId && !r.IsRead).ToListAsync();

            // Loop through all unread messages and update their status.
            foreach (var r in rows)
            {
                r.IsRead = true;
                r.ReadAt = DateTime.UtcNow;
            }

            // Save the changes to the database.
            await _context.SaveChangesAsync();

            // Return a simple success response.
            return Ok();
        }
    }
}
