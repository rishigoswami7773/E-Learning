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
        public string? Role { get; set; } // Admin / Student / Instructor

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
