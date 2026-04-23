using System.ComponentModel.DataAnnotations;

namespace Project_BD.Models
{
    public class AdminProfileUpdateViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be under 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone(ErrorMessage = "Enter a valid mobile number.")]
        [StringLength(20, ErrorMessage = "Mobile number is too long.")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(50)]
        public string Gender { get; set; } = "Other";

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(500, ErrorMessage = "Address must be under 500 characters.")]
        public string Address { get; set; } = string.Empty;

        public string Role { get; set; } = "Admin";
        public string? PhotoPath { get; set; }
    }
}
