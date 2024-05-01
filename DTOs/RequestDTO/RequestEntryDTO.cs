using ia_back.Models;
using System.ComponentModel.DataAnnotations;

namespace ia_back.DTOs.RequestDTO
{
    public class RequestDTO
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ProjectId{ get; set; }
    }
}

