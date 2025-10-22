using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Data.Models
{
    public class UserActivity
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string ActivityType { get; set; } = string.Empty; // VIEW_PRODUCT, CREATE_PRODUCT, etc.

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public int? EntityId { get; set; }

        [MaxLength(100)]
        public string? EntityType { get; set; }

        // Foreign key
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
