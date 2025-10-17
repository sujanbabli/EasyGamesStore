// Standard licensing header automatically added by .NET
// It clarifies that the code is part of the .NET Foundation and licensed under MIT.
#nullable disable

// Importing essential namespaces for ASP.NET Core Identity and MVC functionality
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EasyGamesStore.Areas.Identity.Pages.Account
{
    // Allows users who are not logged in to access this page.
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        // ------------------- Dependency Injections -------------------
        // These private fields store injected services used for handling user management, sign-in,
        // roles, logging, and email confirmations.

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        // Constructor injects all required services from ASP.NET Core’s dependency injection system.
        // These services handle authentication, role management, logging, and email sending.
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore(); // Helper method ensures email storage support
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // ------------------- Properties -------------------

        // InputModel binds form data (Email, Password, ConfirmPassword) from the .cshtml page.
        [BindProperty]
        public InputModel Input { get; set; }

        // The URL to redirect to after registration completes successfully.
        public string ReturnUrl { get; set; }

        // Holds a list of external login providers (e.g., Google, Facebook).
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // ------------------- Nested InputModel Class -------------------
        // Represents the data entered by the user during registration.
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        // ------------------- OnGetAsync() -------------------
        // Called when the Register page is first loaded.
        // It initializes available external login providers (if any).
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // ------------------- OnPostAsync() -------------------
        // Handles the logic when the registration form is submitted.
        // This includes creating the user, assigning roles, and sending confirmation emails.
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // If no return URL is specified, redirect to home page after registration.
            returnUrl ??= Url.Content("~/");

            // Get external authentication schemes again for redisplay if needed.
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Proceed only if form validation passes.
            if (ModelState.IsValid)
            {
                // Create a new IdentityUser object dynamically.
                var user = CreateUser();

                // Assign the entered email as the username and email for the user.
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Create the user account with the provided password.
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // ------------------- Role Handling -------------------
                    // Ensure the "User" role exists. If not, create it.
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    // Assign the newly created user to the "User" role.
                    await _userManager.AddToRoleAsync(user, "User");

                    // ------------------- Email Confirmation -------------------
                    // Generate a unique token for email confirmation.
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Encode the token so it can be safely sent through a URL.
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // Build the confirmation link.
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Send the confirmation email with the encoded link.
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // If email confirmation is required, redirect to confirmation page.
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        // Otherwise, log the user in automatically.
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                // If user creation fails (e.g., weak password or duplicate email), show validation errors.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we reach here, something went wrong — redisplay the form.
            return Page();
        }

        // ------------------- Helper Methods -------------------

        // Dynamically creates a new IdentityUser instance.
        // Using Activator.CreateInstance allows flexibility if custom user classes are later added.
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                // Throws a meaningful error if creation fails due to type or constructor issues.
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Ensures that the user store supports email before proceeding.
        // This prevents runtime errors if an incompatible user store is configured.
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
