using ia_back.DTOs.ProjectDTO;
using ia_back.Models;

namespace ia_back.DTOs.TaskDTO
{
    public class TaskCardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public DeveloperInfo AssignedDev { get; set; }
    }
}
