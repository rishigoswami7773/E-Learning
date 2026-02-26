using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        public ICollection<Option>? Options { get; set; }
    }
}