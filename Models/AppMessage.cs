using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGamesStore.Models
{
    // Enumeration that defines the different categories a message can belong to.
    // This helps organize messages into types such as Promotions, Updates, or Notifications,
    // making it easier for users to filter and for the system to handle them separately.
    public enum MessageCategory { Promotion, Update, Welcome, Notification, Feedback }

    // Represents a message created by the Owner that can be sent to a specific group of users or to all users.
    // Each message can belong to a certain category, have a title, and a formatted HTML body for display in the user's inbox.
    public class AppMessage
    {
        // Primary key of the AppMessage table. Each message record has a unique identifier.
        public int Id { get; set; }

        // The title or subject line of the message. 
        // It is required and limited to a maximum of 120 characters to maintain concise, readable subjects.
        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        // The body content of the message, which supports HTML formatting.
        // This allows messages to include bold text, links, and other styled elements when displayed.
        [Required]
        public string HtmlBody { get; set; } = string.Empty;

        // Defines the category of the message using the MessageCategory enum.
        // Defaults to "Update" if no other category is specified.
        public MessageCategory Category { get; set; } = MessageCategory.Update;

        // Identifies the target audience for the message.
        // Can be "All" (everyone) or specific user tiers like "Platinum", "Gold", "Silver", or "Bronze".
        // This ensures the message is delivered only to the intended user group.
        [Required, StringLength(20)]
        public string TargetTier { get; set; } = "All";

        // Stores the ID of the user who created and sent the message (usually the Owner).
        // The UserId value is a string because ASP.NET Identity uses string-based keys for users.
        [StringLength(450)]
        public string? SenderUserId { get; set; }

        // The date and time the message was created.
        // Automatically initialized to the current UTC time for consistency across time zones.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property that establishes a one-to-many relationship between AppMessage and AppMessageRead.
        // This represents all users who have received or read the message.
        // Useful for tracking message delivery and read analytics.
        public virtual ICollection<AppMessageRead>? Recipients { get; set; } = new List<AppMessageRead>();
    }
}
