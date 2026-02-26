using System.ComponentModel.DataAnnotations;

namespace Project_BD.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [Required]
        public string Description { get; set; }

        public ICollection<Course>? Courses { get; set; }
    }
}
