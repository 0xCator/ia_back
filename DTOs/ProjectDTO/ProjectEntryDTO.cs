using ia_back.Models;
using System.ComponentModel.DataAnnotations;

namespace ia_back.DTOs.ProjectDTO
{
    public class ProjectEntryDTO
    {
        [Required(ErrorMessage = "Project must have a name")]
        public string Name { get; set; }
        [Required]
        public int TeamLeaderID { get; set; }
        public ICollection<string> RequestedDevelopers { get; set; }
    }
}
