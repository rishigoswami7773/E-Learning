using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        [StringLength(150)]
        public string LessonTitle { get; set; }

        [Required]
        public string VideoUrl { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public Module? Module { get; set; }
    }
}