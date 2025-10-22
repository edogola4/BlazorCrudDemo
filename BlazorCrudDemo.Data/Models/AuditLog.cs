using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Data.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT

        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // Product, Category, User, etc.

        public int? EntityId { get; set; }

        [MaxLength(1000)]
        public string? OldValues { get; set; }

        [MaxLength(1000)]
        public string? NewValues { get; set; }

        [MaxLength(500)]
        public string? Changes { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
    }
}
