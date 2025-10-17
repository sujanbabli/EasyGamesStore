using EasyGamesStore.Data;
using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGamesStore.Controllers
{
    // This controller can only be accessed by users with the "Owner" role.
    // It ensures that only the system owner has the right to send or manage messages.
    [Authorize(Roles = "Owner")]
    public class AdminMessagesController : Controller
    {
        // Database context for accessing application data such as messages and recipients.
        // UserManager is used to manage and query identity users and their roles or claims.
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor that injects dependencies. Dependency injection provides the required
        // services (database context and user manager) automatically when the controller is created.
        public AdminMessagesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // This method retrieves a list of all messages that have been sent by the owner.
        // It includes information about recipients and displays the newest messages first.
        public async Task<IActionResult> Index()
        {
            var list = await _context.AppMessages
                .Include(m => m.Recipients)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        // This method displays detailed information about who received a specific message.
        // It shows the user’s email, tier, and whether the message has been read.
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Recipients(int id)
        {
            var message = await _context.AppMessages
                .Include(m => m.Recipients)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound();

            var data = new List<dynamic>();

            // The loop goes through each recipient and collects details such as email and tier.
            foreach (var r in message.Recipients!)
            {
                var user = await _userManager.FindByIdAsync(r.UserId);
                if (user == null) continue;

                var claims = await _userManager.GetClaimsAsync(user);
                var tier = claims.FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";

                data.Add(new
                {
                    r.UserId,
                    user.Email,
                    Tier = tier,
                    IsRead = r.IsRead,
                    r.ReadAt
                });
            }

            ViewBag.MessageTitle = message.Title;
            ViewBag.TotalRecipients = data.Count;
            return View(data);
        }

        // This method simply returns the form page where the owner can compose a new message.
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AppMessage());
        }

        // This method handles form submission when the owner sends a new message.
        // It saves the message to the database and assigns it to the appropriate recipients.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppMessage message)
        {
            if (!ModelState.IsValid) return View(message);

            // The sender’s ID is automatically set based on the currently logged-in owner.
            message.SenderUserId = _userManager.GetUserId(User);

            // The target tier is cleaned and standardized.
            message.TargetTier = NormalizeTier(message.TargetTier);

            // The message is saved in the AppMessages table.
            _context.AppMessages.Add(message);
            await _context.SaveChangesAsync();

            // The next step is to determine which users should receive the message.
            var users = await GetTargetUsersAsync(message.TargetTier);

            // For each targeted user, a new AppMessageRead record is created to track whether they have read it.
            var receipts = users.Select(u => new AppMessageRead
            {
                AppMessageId = message.Id,
                UserId = u.Id,
                IsRead = false
            });
            _context.AppMessageReads.AddRange(receipts);
            await _context.SaveChangesAsync();

            // A confirmation message is shown once the message has been successfully sent.
            TempData["Message"] = $"Message sent to {(message.TargetTier == "All" ? "all users" : $"{message.TargetTier} members")} ({users.Count} recipients).";
            return RedirectToAction(nameof(Index));
        }

        // This helper method ensures that the target tier text (like "gold", "Silver", or "ALL") is
        // converted to a proper standardized form to avoid mismatches.
        private static string NormalizeTier(string input)
        {
            var t = (input ?? "All").Trim();
            return t.Equals("All", StringComparison.OrdinalIgnoreCase) ? "All" :
                   t.Equals("Platinum", StringComparison.OrdinalIgnoreCase) ? "Platinum" :
                   t.Equals("Gold", StringComparison.OrdinalIgnoreCase) ? "Gold" :
                   t.Equals("Silver", StringComparison.OrdinalIgnoreCase) ? "Silver" :
                   t.Equals("Bronze", StringComparison.OrdinalIgnoreCase) ? "Bronze" : "All";
        }

        // This method selects which users should receive a message based on their role or tier.
        // It skips administrative accounts such as Owner or Proprietor and focuses only on end users.
        private async Task<List<IdentityUser>> GetTargetUsersAsync(string targetTier)
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<IdentityUser>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Owners and Proprietors are excluded from receiving any messages.
                if (roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                    roles.Contains("Proprietor", StringComparer.OrdinalIgnoreCase))
                    continue;

                // Case 1: "All" means send to every user except the administrative ones.
                if (targetTier.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    filteredUsers.Add(user);
                    continue;
                }

                // Case 2: "UsersOnly" means send only to users with the "User" role.
                if (targetTier.Equals("UsersOnly", StringComparison.OrdinalIgnoreCase))
                {
                    if (roles.Contains("User", StringComparer.OrdinalIgnoreCase))
                        filteredUsers.Add(user);
                    continue;
                }

                // Case 3: Send to a specific tier such as Bronze, Silver, Gold, etc.
                // The tier is stored as a claim for each user.
                var claims = await _userManager.GetClaimsAsync(user);
                var tier = claims.FirstOrDefault(c => c.Type == "Tier")?.Value ?? "Bronze";

                if (tier.Equals(targetTier, StringComparison.OrdinalIgnoreCase))
                    filteredUsers.Add(user);
            }

            return filteredUsers;
        }
    }
}
