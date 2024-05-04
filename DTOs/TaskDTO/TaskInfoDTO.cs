using ia_back.DTOs.CommentDTO;
using ia_back.DTOs.ProjectDTO;
using ia_back.Models;

namespace ia_back.DTOs.TaskDTO
{
    public class TaskInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public ProjectStatus Status { get; set; }
        public DeveloperInfo AssignedDev { get; set; }
    }
}
