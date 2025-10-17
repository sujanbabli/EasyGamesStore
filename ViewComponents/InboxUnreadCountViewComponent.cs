using EasyGamesStore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EasyGamesStore.ViewComponents
{
    // This ViewComponent dynamically displays the number of unread messages
    // for the currently logged-in user inside the website’s navigation bar or dashboard.
    //
    // ViewComponents are reusable UI elements that can run server-side logic and return partial views.
    // This one helps notify users when they have new or unread messages in their Inbox.
    public class InboxUnreadCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor injects the database context and user manager
        // so the component can identify the current user and query their message records.
        public InboxUnreadCountViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // This method runs automatically when the component is invoked in a Razor view.
        // It returns a partial view that displays the count of unread messages.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // If no user is logged in, skip rendering (prevents errors on anonymous pages).
            if (!User.Identity.IsAuthenticated)
                return Content(string.Empty);

            // Get the currently authenticated user from the Identity system.
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user == null)
                return Content(string.Empty);

            // Count how many messages are unread for this specific user.
            // The AppMessageReads table stores which messages have been read or not.
            var count = await _context.AppMessageReads
                .CountAsync(r => r.UserId == user.Id && !r.IsRead);

            // Return the partial view and pass the count value to it.
            // This allows the number to be shown as a small badge or notification icon in the UI.
            return View(count);
        }
    }
}
