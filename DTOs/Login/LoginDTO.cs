using System.ComponentModel.DataAnnotations;

namespace ia_back.DTOs.Login
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
