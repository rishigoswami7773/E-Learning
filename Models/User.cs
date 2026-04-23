using System.ComponentModel.DataAnnotations;

namespace Project_BD.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PhotoPath { get; set; }

        [Required]
        public string? Role { get; set; } = "Student"; // Admin / Student / Instructor

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
