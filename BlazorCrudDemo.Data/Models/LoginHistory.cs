using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorCrudDemo.Data.Models
{
    public class LoginHistory
    {
        public int Id { get; set; }

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        public DateTime? LogoutTime { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public bool IsSuccessful { get; set; } = true;

        [MaxLength(200)]
        public string? FailureReason { get; set; }

        public TimeSpan? SessionDuration { get; set; }

        // Foreign key - make this required and don't default to empty string
        [Required]
        public string UserId { get; set; } = default!;

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
