using ia_back.Models;

namespace ia_back.DTOs.ProjectDTO
{
    public class ProjectEntryDTO
    {
        public string Name { get; set; }
        public int TeamLeaderID { get; set; }
        public ICollection<string> RequestedDevelopers { get; set; }
    }
}
