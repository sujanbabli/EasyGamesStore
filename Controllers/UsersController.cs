using EasyGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasyGamesStore.Controllers
{
    // This controller is restricted to "Owner" role only.
    // That means only Owners can create, edit, or delete users.
    [Authorize(Roles = "Owner")]
    public class UsersController : Controller
    {
        // UserManager and RoleManager are services provided by ASP.NET Identity.
        // UserManager helps us manage user accounts (create, edit, delete).
        // RoleManager helps us manage roles (Owner, Customer, etc.).
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor injects the services so we can use them in this controller.
        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        // This lists all users along with the roles assigned to them.
        // We create a custom view model (UserRolesViewModel) so we can pass
        // both the user details and their roles into the view.
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();

            // For each user in the system, we also load their assigned roles.
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        // GET: Users/Create
        // Displays a blank form for creating a new user.
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // POST: Users/Create
        // This method handles the form submission for creating a new user.
        // It checks if the model is valid, then uses UserManager to create the account.
        // If a role is specified, and it exists, the user is added to that role.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // ✅ Ensure role exists before assigning
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["Message"] = $"✅ User created successfully as {model.Role}.";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Users/Edit/5
        // Loads a user by ID and prepares a view model with their current roles.
        // Also gets a list of all available roles, so the owner can assign or remove them.
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                Roles = roles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        // POST: Users/Edit
        // This method updates user details and manages their role assignments.
        // It first updates the email/username, then calculates which roles to add/remove.
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model, string[] selectedRoles)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;

            // Find the difference between currently assigned roles and newly selected roles.
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = selectedRoles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(selectedRoles);

            // Update the roles accordingly.
            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            await _userManager.UpdateAsync(user);

            // Using TempData here so we can show a success message after redirect.
            TempData["Message"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Delete
        // Deletes a user from the system by ID.
        // For now, it just removes the account completely. In the future,
        // we might want to consider soft deletes (marking the user inactive instead).
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }
    }
}
