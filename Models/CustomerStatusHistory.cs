using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGamesStore.Models
{
    // This model records and tracks the history of a customer's status or tier
    // based on their total purchase value within the EasyGamesStore system.
    // It helps monitor user progress and can be used to update loyalty levels (e.g., Bronze, Silver, Gold, etc.).
    public class CustomerStatusHistory
    {
        // Primary key for the CustomerStatusHistory table.
        // Each record represents one snapshot of a user's purchase and status at a particular time.
        public int Id { get; set; }

        // The unique identifier of the user this record belongs to.
        // This links directly to the IdentityUser table in ASP.NET Identity.
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Represents the user's tier or membership status at the time of record creation.
        // The default value is "Regular", but it can be updated to reflect the user's progress:
        // e.g., Bronze, Silver, Gold, or Platinum depending on total purchases.
        [Required]
        public string Status { get; set; } = "Regular";

        // The total amount (in currency) that the user has spent in their lifetime on the platform.
        // This value helps determine which tier the user belongs to.
        // It uses a range validation to ensure no negative values are stored.
        [Range(0, double.MaxValue)]
        public decimal TotalPurchase { get; set; } = 0;

        // The date and time when this record was last updated.
        // This field automatically records when the user's status or purchase total changes.
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
