using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class Option
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        public string OptionText { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }
    }
}