using System.ComponentModel.DataAnnotations;

namespace UserManager.Models
{
    public enum UserStatus
    {
        Unverified = 0,
        Active = 1,
        Blocked = 2
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserStatus Status { get; set; } = UserStatus.Unverified;

        public UserStatus? PreviousStatus { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}