using ia_back.Models;

namespace ia_back.DTOs.ProjectDTO
{
    public class ProjectInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TeamLeaderId { get; set; }
        public ICollection<DeveloperInfo> AssignedDevelopers { get; set; }
    }

    public class DeveloperInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
    }
}
