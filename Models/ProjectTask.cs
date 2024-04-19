using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ia_back.Models
{
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public string Attachment { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [ForeignKey("AssignedDev")]
        public int AssignedDevId { get; set; }
        public User AssignedDev { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }

    public enum ProjectStatus
    {
        ToDo,
        InProgress,
        Done
    }
}
