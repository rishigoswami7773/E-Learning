using System.ComponentModel.DataAnnotations;

namespace Project_BD.Models
{
    public class Student
    {
        [Key]
        public int Student_roll {  get; set; }
        [Required(ErrorMessage ="Name Required")]
        public string Student_name {  get; set; }
        [Required(ErrorMessage ="Enter the date of birth")]
        [Range(18,45)]
        public DateOnly DOB { get; set; }
        public string Gender { get; set; }
    }
}
