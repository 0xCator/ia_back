using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ia_back.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("TeamLeader")]
        public int TeamLeaderId { get; set; }
        public User TeamLeader { get; set; }

        public ICollection<User> AssignedDevelopers { get; set; }
        public ICollection<User> RequestedDevelopers { get; set; }


        public ICollection<ProjectTask> Tasks { get; set; }
    }
}
