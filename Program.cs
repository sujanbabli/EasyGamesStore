using EasyGamesStore.Data;      // Importing the ApplicationDbContext and database-related classes
using EasyGamesStore.DataSeed;  // Importing seeding logic for roles/admin user
using Microsoft.AspNetCore.Identity; // Identity for user management
using Microsoft.EntityFrameworkCore; // EF Core for database access

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); // Create a new WebApplicationBuilder

        // -------------------- DATABASE CONNECTION --------------------
        // Get the connection string from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register ApplicationDbContext with SQL Server using EF Core
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // This adds detailed exception pages for database-related errors during development
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        // ---------------------------------------------------------------

        // -------------------- IDENTITY & ROLES --------------------
        // Add default Identity services (user login, registration, etc.)
        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                options.SignIn.RequireConfirmedAccount = true) // Force email confirmation
            .AddRoles<IdentityRole>() // Enable role management
            .AddEntityFrameworkStores<ApplicationDbContext>(); // Store users and roles in EF Core DB
        // ----------------------------------------------------------

        builder.Services.AddControllersWithViews(); // Add MVC support for controllers and views

        // -------------------- SESSION CONFIGURATION --------------------
        // Session is used to store temporary data like shopping cart counts

        // The following session configuration was added with guidance from ChatGPT

        builder.Services.AddDistributedMemoryCache(); // Required in-memory cache for session
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 minutes of inactivity
            options.Cookie.HttpOnly = true; // Prevent JavaScript access for security
            options.Cookie.IsEssential = true; // Marks cookie essential for GDPR compliance
        });
        // ---------------------------------------------------------------

        builder.Services.AddHttpContextAccessor(); // Allows access to HttpContext in services (e.g., cart count in _Layout)

        var app = builder.Build(); // Build the app

        // -------------------- HTTP REQUEST PIPELINE --------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint(); // Show migration pages during development
        }
        else
        {
            app.UseExceptionHandler("/Home/Error"); // Custom error page in production
            app.UseHsts(); // HTTP Strict Transport Security
        }

        app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
        app.UseStaticFiles(); // Serve static files like CSS, JS, images

        // This middleware placement was clarified with ChatGPT’s assistance

        app.UseSession(); // Must be before UseRouting to enable session in controllers/views

        app.UseRouting(); // Adds routing middleware

        app.UseAuthentication(); // Authenticate users before authorization
        app.UseAuthorization();  // Enforce access control

        // Default route configuration for MVC controllers
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages(); // Enable Razor Pages for Identity UI

        // -------------------- SEED ROLES AND ADMIN USER --------------------
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                // Seed predefined roles (e.g., Admin, User) and create an initial admin user
                await RoleSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error seeding roles: " + ex.Message); // Log any errors during seeding
            }
        }
        // ---------------------------------------------------------------

        await app.RunAsync(); // Run the web application
    }
}
