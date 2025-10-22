using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Audit fields
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation properties
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();
        public ICollection<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();
    }
}
