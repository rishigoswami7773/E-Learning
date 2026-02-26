using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class QuizAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        [Required]
        public double Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}
