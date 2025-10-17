using System.ComponentModel.DataAnnotations;

namespace EasyGamesStore.Models
{
    // This ViewModel is used when creating a new user in the system.
    // It ensures that the necessary information like email, password, and role are provided.
    public class CreateUserViewModel
    {
        // The email of the new user.
        // Required field with email validation to prevent invalid email formats.
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        // The password for the new user.
        // Must be at least 6 characters long, and uses the Password data type for security.
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = "";

        // The role assigned to the new user (e.g., Owner, User).
        // This ensures that every new user has a role for proper access control.
        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; } = "";
    }

    // This ViewModel is used to display a list of users along with their assigned roles.
    // Useful for admin interfaces to manage users and their permissions.
    public class UserRolesViewModel
    {
        // Unique ID of the user.
        public string Id { get; set; } = "";

        // Email of the user, used as the primary identifier.
        public string Email { get; set; } = "";

        // List of roles assigned to this user.
        // This allows quick reference of what permissions the user has.
        public IList<string> Roles { get; set; } = new List<string>();
    }

    // This ViewModel is used when editing an existing user.
    // It allows updating the user's email and roles.
    public class EditUserViewModel
    {
        // Unique ID of the user being edited.
        [Required]
        public string Id { get; set; } = "";

        // The user's email, which can be updated.
        // Validation ensures it is a proper email address.
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        // Current roles assigned to the user.
        // This helps pre-populate the form so the admin can modify roles easily.
        public List<string> Roles { get; set; } = new List<string>();

        // List of all available roles in the system.
        // This allows the admin to select roles from the full list when editing.
        public List<string> AllRoles { get; set; } = new List<string>();
    }
}
