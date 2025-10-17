using Microsoft.AspNetCore.Identity;

namespace EasyGamesStore.DataSeed
{
    // This static class is responsible for creating essential user roles and default accounts
    // when the application starts for the first time. It ensures that the system has a base structure
    // for role-based access control before any users interact with it.
    public static class RoleSeeder
    {
        // The SeedRolesAndAdminAsync method is called during application startup.
        // It checks for the existence of key roles and a default Owner account.
        // If any of them are missing, it creates them automatically.
        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            // Define the core roles required for the system.
            // These roles correspond to the different access levels and responsibilities within EasyGamesStore.
            string[] roleNames = { "Owner", "Admin", "Proprietor", "User" };

            // Iterate through each role name and create the role if it does not already exist.
            // This ensures that even if the database is freshly created, all required roles will be available.
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Create the default "Owner" account if it does not already exist.
            // The Owner acts as the system administrator and has full access to all parts of the application.
            string ownerEmail = "owner@easygamesstore.com";
            string ownerPassword = "Owner@123";

            // Check if the Owner user already exists in the database.
            var ownerUser = await userManager.FindByEmailAsync(ownerEmail);

            // If no such user exists, create a new Owner account and assign the Owner role to it.
            if (ownerUser == null)
            {
                var newOwner = new IdentityUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    EmailConfirmed = true // Mark the account as verified so it can be used immediately.
                };

                var result = await userManager.CreateAsync(newOwner, ownerPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newOwner, "Owner");
            }

            // Optionally create a sample Proprietor account to simplify testing.
            // This user represents a shop owner or manager who operates under the Owner.
            string propEmail = "shop1@easygamesstore.com";
            string propPassword = "Shop@123";

            var propUser = await userManager.FindByEmailAsync(propEmail);

            // If the test Proprietor account does not exist, create it and assign the correct role.
            if (propUser == null)
            {
                var newProp = new IdentityUser
                {
                    UserName = propEmail,
                    Email = propEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newProp, propPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newProp, "Proprietor");
            }
        }
    }
}
