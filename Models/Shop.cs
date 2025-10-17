using System.ComponentModel.DataAnnotations;

namespace EasyGamesStore.Models
{
    // The Shop class represents an individual retail shop or outlet within the EasyGamesStore system.
    // Each shop can be assigned to a proprietor (shop manager) and contains key details such as
    // name, address, contact information, and proprietor credentials.
    public class Shop
    {
        // Primary key for the Shop table. Each shop record is uniquely identified by this ID.
        public int Id { get; set; }

        // The name of the shop. This is required and limited to 100 characters
        // to ensure that shop names remain concise and easy to display in the system.
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // The physical address of the shop. This field is required to locate the store
        // and is limited to 200 characters for practical data storage and readability.
        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        // The primary contact phone number of the shop.
        // This allows communication with the shop directly when needed.
        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        // The email address of the shop proprietor (store manager).
        // This is optional because a proprietor may not be assigned when the shop is first created.
        [StringLength(100)]
        [EmailAddress]
        public string? ProprietorEmail { get; set; }

        // The unique user ID of the proprietor from the Identity system.
        // This links the shop to an IdentityUser record once the proprietor is assigned.
        [StringLength(450)]
        public string? ProprietorUserId { get; set; }

        // Optional password field used when creating or assigning a new proprietor manually.
        // This field is not typically stored permanently for security reasons but may be used
        // during setup to initialize a new proprietor account.
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string? ProprietorPassword { get; set; }

        // Overrides the default ToString() method to provide a more readable format
        // when displaying a shop object in logs or dropdowns.
        // If no proprietor is assigned, it clearly indicates that status.
        public override string ToString() => $"{Name} ({ProprietorEmail ?? "No Proprietor"})";
    }
}
