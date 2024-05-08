using System.ComponentModel.DataAnnotations;
using ia_back.Models;

namespace ia_back.DTOs.Login
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(256, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Role Role { get; set; }
    }
}
