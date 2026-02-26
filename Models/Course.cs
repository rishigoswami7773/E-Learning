using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Project_BD.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Module>? Modules { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}