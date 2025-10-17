using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGamesStore.Models
{
    // Represents a record that links a specific AppMessage to a specific user,
    // tracking whether the message has been read by that user and when.
    // This model supports the analytics and user inbox features in the messaging system.
    public class AppMessageRead
    {
        // Primary key for the AppMessageRead table. Each record represents one user's read status for a message.
        public int Id { get; set; }

        // The foreign key that links this record to the AppMessage it corresponds to.
        // This ensures each read record belongs to a specific message.
        [Required]
        public int AppMessageId { get; set; }

        // Navigation property that establishes the relationship between AppMessageRead and AppMessage.
        // It allows access to message details when querying read records.
        [ForeignKey(nameof(AppMessageId))]
        public AppMessage? AppMessage { get; set; }

        // The ID of the user who received the message.
        // String length is limited to 450 characters to match ASP.NET Identity’s user ID format.
        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Indicates whether the user has read the message or not.
        // Defaults to false, and is updated to true when the user opens or views the message.
        public bool IsRead { get; set; } = false;

        // Stores the exact date and time when the user read the message.
        // This field remains null until the message is actually opened by the user.
        public DateTime? ReadAt { get; set; }

        // A temporary property used for displaying the recipient’s email address in the user interface.
        // Marked with [NotMapped] to ensure it is not stored in the database.
        [NotMapped]
        public string? UserEmail { get; set; }
    }
}
