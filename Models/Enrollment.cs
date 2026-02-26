using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    }
}