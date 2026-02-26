using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_BD.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(150)]
        public string ModuleTitle { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public ICollection<Lesson>? Lessons { get; set; }
    }
}
