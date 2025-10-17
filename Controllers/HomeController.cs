using System.Diagnostics; // I use this to get activity details, which helps when showing error information.
using EasyGamesStore.Models; // This gives me access to the ErrorViewModel class I use in the Error page.
using Microsoft.AspNetCore.Mvc; // This is needed because my controller inherits from Controller and uses MVC features.

namespace EasyGamesStore.Controllers
{
    // This controller handles basic pages of my website like home, privacy, and error.
    public class HomeController : Controller
    {
        // I use a logger here so I can record useful information when something goes wrong.
        // Instead of creating it manually, it is injected automatically by ASP.NET Core (dependency injection).
        // In the future, I can use this to log important events, warnings, or errors to files or monitoring tools.
        private readonly ILogger<HomeController> _logger;

        // The constructor sets up the logger when the controller is created.
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // This is the method for the homepage.
        // Right now it just loads the Index view, which could be a welcome page or a dashboard.
        // Later, I might use this method to show featured games or dynamic content from a database.
        public IActionResult Index()
        {
            return View();
        }

        // This shows the Privacy page.
        // It’s a simple static page for now, but if the privacy policy changes often,
        // I could pull this text from a database or CMS instead of keeping it fixed in one file.
        public IActionResult Privacy()
        {
            return View();
        }

        // This method is for the Error page. It is important because it handles unexpected problems.
        // I pass an ErrorViewModel to the view that contains a RequestId.
        // The RequestId is useful because:
        //   - It helps track the specific request in the logs.
        //   - If a user reports a problem, I can search for this ID in the logs and see what went wrong.
        //
        // The ResponseCache attribute is also important:
        //   - It makes sure the error page is never cached, so users always see the latest error info.
        //
        // In the future, I could expand this to log more details with _logger.LogError,
        // or show different pages depending on the type of error (for example: 404 Not Found vs. 500 Server Error).
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
