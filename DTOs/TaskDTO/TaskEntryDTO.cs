using System.ComponentModel.DataAnnotations;

namespace ia_back.DTOs.TaskDTO
{
    public class TaskEntryDTO
    {
        [Required(ErrorMessage = "Task must have a name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Task must have a description")]
        public string Description { get; set; }
        [Required]
        public int ProjectID { get; set; }
        [Required]
        public int AssignedDevId { get; set; }
    }
}
